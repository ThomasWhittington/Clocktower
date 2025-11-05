namespace Clocktower.Server.Common;

public static class Validation
{
    public static bool BeValidDiscordSnowflake(ulong value)
    {
        return value > 41943040000L;
    }

    public static bool BeValidDiscordSnowflake(string value)
    {
        var parses = ulong.TryParse(value, out var val);
        if (!parses) return false;
        return BeValidDiscordSnowflake(val);
    }
}