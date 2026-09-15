using System.Security.Cryptography;
using System.Text;

namespace VoteBattle.Infrastructure.Security;

/// <summary>
/// Generates and hashes opaque, single-use tokens (e.g. email verification).
/// The raw token is sent to the user; only its hash is stored.
/// </summary>
public static class TokenHasher
{
    /// <summary>Generates a cryptographically random, URL-safe token.</summary>
    public static string GenerateToken(int byteLength = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Base64UrlEncode(bytes);
    }

    /// <summary>Returns the SHA-256 hash (hex) of a token, for storage/lookup.</summary>
    public static string Hash(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}
