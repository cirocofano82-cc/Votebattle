using System.Globalization;
using System.Text;

namespace VoteBattle.Infrastructure.Common;

/// <summary>
/// Produces URL-friendly slugs (e.g. "Samsung Galaxy Fold 8 vs iPhone Duo"
/// => "samsung-galaxy-fold-8-vs-iphone-duo").
/// </summary>
public static class SlugGenerator
{
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Normalize and strip diacritics.
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }

        var cleaned = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        // Replace any run of non-alphanumeric characters with a single hyphen.
        var result = new StringBuilder(cleaned.Length);
        var lastWasHyphen = false;
        foreach (var ch in cleaned)
        {
            if (char.IsLetterOrDigit(ch))
            {
                result.Append(ch);
                lastWasHyphen = false;
            }
            else if (!lastWasHyphen)
            {
                result.Append('-');
                lastWasHyphen = true;
            }
        }

        return result.ToString().Trim('-');
    }
}
