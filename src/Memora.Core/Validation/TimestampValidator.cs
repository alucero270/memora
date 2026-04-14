using System.Globalization;
using System.Text.RegularExpressions;

namespace Memora.Core.Validation;

public static partial class TimestampValidator
{
    [GeneratedRegex("(Z|[+-]00:00)$", RegexOptions.CultureInvariant)]
    private static partial Regex UtcSuffixPattern();

    public static bool TryParseUtc(string? value, out DateTimeOffset timestamp)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !UtcSuffixPattern().IsMatch(value) ||
            !DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp))
        {
            timestamp = default;
            return false;
        }

        if (timestamp.Offset != TimeSpan.Zero)
        {
            timestamp = default;
            return false;
        }

        return true;
    }
}
