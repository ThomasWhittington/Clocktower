namespace Clocktower.Server.Common;

public static class Validation
{
    public static bool BeValidDiscordSnowflake(string value)
    {
        if (!ulong.TryParse(value, out var val)) return false;
        return val > 41943040000L;
    }
}