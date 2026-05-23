using System.Globalization;
using System.Text;

namespace LendingSystem.SharedKernel.Application.Common;

public static class PublicResourceKey
{
    public static string FromInt(string prefix, int value)
    {
        var raw = Encoding.UTF8.GetBytes(value.ToString(CultureInfo.InvariantCulture));
        return $"{prefix}_{Convert.ToBase64String(raw).TrimEnd('=').Replace('+', '-').Replace('/', '_')}";
    }

    public static bool TryGetInt(string prefix, string? key, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(key) || !key.StartsWith($"{prefix}_", StringComparison.Ordinal))
        {
            return false;
        }

        var payload = key[(prefix.Length + 1)..].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

        try
        {
            var text = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
