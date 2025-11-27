using Clocktower.Server.Data;

namespace Clocktower.ServerTests.TestHelpers;

public static class CommonMethods
{
    public static string GetRandomString() => Guid.NewGuid().ToString();
    public static string GetRandomSnowflakeStringId() => GetRandomSnowflakeNumberId().ToString();
    public static ulong GetRandomSnowflakeNumberId() => (ulong)new Random((int)DateTime.Now.Ticks).NextInt64();
    public static GameUser GetRandomGameUser() => new(GetRandomSnowflakeStringId(), GetRandomString(), GetRandomString());
}