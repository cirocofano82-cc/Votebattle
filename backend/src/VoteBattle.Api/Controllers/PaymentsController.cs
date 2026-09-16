using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using VoteBattle.Core.Common;
using VoteBattle.Core.DTOs.Payments;
using VoteBattle.Core.Interfaces;

namespace VoteBattle.Api.Controllers;

[Route("api/payments")]
public class PaymentsController : ApiControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IStripeWebhookService _webhookService;

    public PaymentsController(IPaymentService paymentService, IStripeWebhookService webhookService)
    {
        _paymentService = paymentService;
        _webhookService = webhookService;
    }

    /// <summary>Starts a Stripe Checkout Session for a vote package. Returns the redirect URL.</summary>
    [HttpPost("checkout")]
    [Authorize]
    [EnableRateLimiting("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CreateCheckoutRequest request, CancellationToken ct)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized(ApiResponse.Fail("Not authenticated."));

        var result = await _paymentService.CreateCheckoutSessionAsync(userId.Value, request.VotePackageId, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Stripe webhook endpoint. Reads the raw body for signature verification.
    /// This is the source of truth for payment confirmation.
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    [EnableRateLimiting("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();

        var result = await _webhookService.ProcessAsync(json, signature, ct);

        // Always return 200 for successfully handled (or duplicate) events so Stripe
        // stops retrying; return 400 only when the signature is invalid.
        if (result.Succeeded)
            return Ok(ApiResponse.Ok(result.Message));

        return BadRequest(ApiResponse.Fail(result.Message, result.Errors));
    }
}
