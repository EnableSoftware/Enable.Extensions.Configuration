namespace Enable.Extensions.Configuration;

internal static class StringExtensions
{
    public static string ReplaceIfPresent(this string @this, string placeholder, Lazy<string> value)
    {
        if (string.IsNullOrWhiteSpace(@this)
            || string.IsNullOrWhiteSpace(placeholder)
            || !@this.Contains(placeholder))
        {
            return @this;
        }

        return @this.Replace(
            placeholder,
            value.Value.Replace(' ', '_'));
    }

    public static string ReplaceSpaces(this string @this) => @this.Replace(' ', '_');
}
