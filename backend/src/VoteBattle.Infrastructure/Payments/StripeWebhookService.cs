using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Stripe;
using Stripe.Checkout;
using VoteBattle.Core.Common;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;
using VoteBattle.Core.Options;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure.Payments;

/// <summary>
/// Verifies and processes Stripe webhooks. The webhook is the source of truth for
/// payment confirmation. Processing is idempotent: a duplicated event never grants
/// credits twice (guarded by the ProcessedStripeEvent primary key and by the payment
/// status).
/// </summary>
public class StripeWebhookService : IStripeWebhookService
{
    private readonly AppDbContext _db;
    private readonly StripeOptions _stripe;
    private readonly IAuditService _audit;
    private readonly ILogger<StripeWebhookService> _logger;

    public StripeWebhookService(
        AppDbContext db,
        IOptions<StripeOptions> stripe,
        IAuditService audit,
        ILogger<StripeWebhookService> logger)
    {
        _db = db;
        _stripe = stripe.Value;
        _audit = audit;
        _logger = logger;
    }

    public async Task<Result> ProcessAsync(string requestJson, string? signatureHeader, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(signatureHeader))
            return Result.Failure(ErrorType.Validation, "Missing Stripe signature.");

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                requestJson, signatureHeader, _stripe.WebhookSecret,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Stripe webhook signature verification failed.");
            return Result.Failure(ErrorType.Validation, "Invalid webhook signature.");
        }

        var now = DateTimeOffset.UtcNow;
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        // Idempotency guard: the event id is the primary key.
        try
        {
            _db.ProcessedStripeEvents.Add(new ProcessedStripeEvent
            {
                StripeEventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                ProcessedAt = now
            });
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Duplicate event: another delivery already processed it. Roll back and
            // drop the failed Added entity so it is not retried by the next SaveChanges.
            await tx.RollbackAsync(ct);
            _db.ChangeTracker.Clear();
            await _audit.LogAsync(AuditEventType.StripeWebhookDuplicate, null, null,
                new { eventId = stripeEvent.Id }, ct);
            return Result.Success("Duplicate event ignored.");
        }

        try
        {
            if (stripeEvent.Type == "checkout.session.completed" &&
                stripeEvent.Data.Object is Session session)
            {
                await HandleCheckoutCompletedAsync(session, now, ct);
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            await _audit.LogAsync(AuditEventType.StripeWebhookProcessed, null, null,
                new { eventId = stripeEvent.Id, type = stripeEvent.Type }, ct);

            return Result.Success("Processed.");
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _logger.LogError(ex, "Failed to process Stripe event {EventId}.", stripeEvent.Id);
            throw;
        }
    }

    private async Task HandleCheckoutCompletedAsync(Session session, DateTimeOffset now, CancellationToken ct)
    {
        var payment = await FindPaymentAsync(session, ct);
        if (payment is null)
        {
            _logger.LogWarning("No matching payment for checkout session {SessionId}.", session.Id);
            return;
        }

        // Double-safety idempotency: never grant twice.
        if (payment.Status == PaymentStatus.Paid)
            return;

        payment.Status = PaymentStatus.Paid;
        payment.StripePaymentIntentId = session.PaymentIntentId;
        payment.UpdatedAt = now;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == payment.UserId, ct);
        if (user is not null)
        {
            var newBalance = user.VoteCredits + payment.VoteCreditsPurchased;
            user.VoteCredits = newBalance;
            user.UpdatedAt = now;

            _db.VoteCreditTransactions.Add(new VoteCreditTransaction
            {
                UserId = user.Id,
                Type = CreditTransactionType.Purchase,
                Amount = payment.VoteCreditsPurchased,
                BalanceAfter = newBalance,
                ReferenceType = "Payment",
                ReferenceId = payment.Id.ToString(),
                CreatedAt = now
            });
        }

        await _audit.LogAsync(AuditEventType.PaymentCompleted, payment.UserId, null,
            new { paymentId = payment.Id, credits = payment.VoteCreditsPurchased }, ct);
    }

    private async Task<Payment?> FindPaymentAsync(Session session, CancellationToken ct)
    {
        if (session.Metadata is not null &&
            session.Metadata.TryGetValue("paymentId", out var paymentIdStr) &&
            Guid.TryParse(paymentIdStr, out var paymentId))
        {
            var byId = await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId, ct);
            if (byId is not null)
                return byId;
        }

        return await _db.Payments.FirstOrDefaultAsync(p => p.StripeCheckoutSessionId == session.Id, ct);
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: "23505" };
}
