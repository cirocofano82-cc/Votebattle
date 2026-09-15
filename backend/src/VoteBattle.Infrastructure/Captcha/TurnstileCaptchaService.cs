using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoteBattle.Core.Interfaces;
using VoteBattle.Core.Options;

namespace VoteBattle.Infrastructure.Captcha;

/// <summary>
/// Verifies Cloudflare Turnstile tokens server-side. When disabled (development),
/// verification always succeeds without any network call.
/// </summary>
public class TurnstileCaptchaService : ICaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly TurnstileOptions _options;
    private readonly ILogger<TurnstileCaptchaService> _logger;

    public TurnstileCaptchaService(
        HttpClient httpClient,
        IOptions<TurnstileOptions> options,
        ILogger<TurnstileCaptchaService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> VerifyAsync(string? token, string? remoteIp, CancellationToken ct = default)
    {
        // CAPTCHA disabled (development): accept everything.
        if (!_options.Enabled)
            return true;

        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var form = new List<KeyValuePair<string, string>>
            {
                new("secret", _options.SecretKey),
                new("response", token)
            };
            if (!string.IsNullOrWhiteSpace(remoteIp))
                form.Add(new("remoteip", remoteIp));

            using var content = new FormUrlEncodedContent(form);
            using var response = await _httpClient.PostAsync(_options.VerifyUrl, content, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TurnstileVerifyResponse>(cancellationToken: ct);
            return result?.Success ?? false;
        }
        catch (Exception ex)
        {
            // Fail closed: if verification cannot complete, treat as failed.
            _logger.LogWarning(ex, "Turnstile verification failed to complete.");
            return false;
        }
    }

    private sealed class TurnstileVerifyResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; }
    }
}
