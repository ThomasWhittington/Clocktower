namespace Clocktower.ServerTests.TestHelpers;

public static class CommonMethods
{
    public static string GetRandomStringId() => Guid.NewGuid().ToString();
    public static string GetRandomSnowflakeId() => new Random((int)DateTime.Now.Ticks).NextInt64().ToString();
}