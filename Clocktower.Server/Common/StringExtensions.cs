namespace Clocktower.Server.Common;

public static class StringExtension
{
    extension(string str)
    {
        public string ToCamelCase() =>
            string.IsNullOrEmpty(str) || str.Length < 2
                ? str.ToLowerInvariant()
                : char.ToLowerInvariant(str[0]) + str[1..];
    }
}