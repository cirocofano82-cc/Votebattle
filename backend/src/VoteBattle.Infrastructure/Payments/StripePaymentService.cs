using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Payments;
using VoteBattle.Core.Entities;
using VoteBattle.Core.Enums;
using VoteBattle.Core.Interfaces;
using VoteBattle.Core.Options;
using VoteBattle.Infrastructure.Data;

namespace VoteBattle.Infrastructure.Payments;

/// <summary>
/// Stripe implementation of <see cref="IPaymentService"/>. Creates a Payment row
/// (PENDING) and a Stripe Checkout Session carrying identifying metadata. The
/// purchase is only confirmed later by the verified webhook.
/// </summary>
public class StripePaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly StripeOptions _stripe;
    private readonly AuthOptions _auth;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(
        AppDbContext db,
        IOptions<StripeOptions> stripe,
        IOptions<AuthOptions> auth,
        ILogger<StripePaymentService> logger)
    {
        _db = db;
        _stripe = stripe.Value;
        _auth = auth.Value;
        _logger = logger;
    }

    public async Task<Result<CheckoutSessionDto>> CreateCheckoutSessionAsync(
        Guid userId, int votePackageId, CancellationToken ct = default)
    {
        // Fail fast (and create no payment row) when Stripe isn't configured yet.
        if (string.IsNullOrWhiteSpace(_stripe.SecretKey))
        {
            _logger.LogWarning("Checkout requested but Stripe is not configured (missing secret key).");
            return Result<CheckoutSessionDto>.Failure(ErrorType.Validation,
                "Payments aren't available right now. Please try again later.");
        }

        var package = await _db.VotePackages
            .FirstOrDefaultAsync(p => p.Id == votePackageId && p.IsActive, ct);
        if (package is null)
            return Result<CheckoutSessionDto>.Failure(ErrorType.Validation, "This package is not available.");

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
            return Result<CheckoutSessionDto>.Failure(ErrorType.Unauthorized, "Not authenticated.");
        if (user.Status != UserStatus.Active)
            return Result<CheckoutSessionDto>.Failure(ErrorType.Forbidden, "Your account cannot purchase credits.");

        // Create the pending payment first (snapshot the credits/price).
        var now = DateTimeOffset.UtcNow;
        var payment = new Payment
        {
            UserId = userId,
            VotePackageId = package.Id,
            Amount = package.Price,
            Currency = package.Currency,
            Status = PaymentStatus.Pending,
            VoteCreditsPurchased = package.Credits,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        var frontend = _auth.FrontendUrl.TrimEnd('/');
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            ClientReferenceId = userId.ToString(),
            SuccessUrl = $"{frontend}/credits?checkout=success&session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{frontend}/credits?checkout=cancel",
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = package.Currency.ToLowerInvariant(),
                        UnitAmount = (long)Math.Round(package.Price * 100m),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"{package.Name} ({package.Credits} Vote Credits)"
                        }
                    }
                }
            },
            Metadata = new Dictionary<string, string>
            {
                ["paymentId"] = payment.Id.ToString(),
                ["userId"] = userId.ToString(),
                ["votePackageId"] = package.Id.ToString()
            }
        };

        var requestOptions = new RequestOptions { ApiKey = _stripe.SecretKey };

        Session session;
        try
        {
            session = await new SessionService().CreateAsync(options, requestOptions, ct);
        }
        catch (Exception ex)
        {
            // Any failure (Stripe error, network/proxy issue) must not surface as a 500.
            // The checkout never started, so drop the pending payment instead of leaving
            // an orphan row that would clutter the user's purchase history.
            _logger.LogError(ex, "Stripe checkout session creation failed for user {UserId}.", userId);
            _db.Payments.Remove(payment);
            await _db.SaveChangesAsync(ct);
            return Result<CheckoutSessionDto>.Failure(ErrorType.Validation,
                "Could not start checkout. Please try again.");
        }

        payment.StripeCheckoutSessionId = session.Id;
        payment.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Result<CheckoutSessionDto>.Success(new CheckoutSessionDto
        {
            SessionId = session.Id,
            Url = session.Url
        });
    }
}
