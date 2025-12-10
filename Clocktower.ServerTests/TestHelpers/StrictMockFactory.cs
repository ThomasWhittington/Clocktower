namespace Clocktower.ServerTests.TestHelpers;

public static class StrictMockFactory
{
    public static Mock<T> Create<T>() where T : class
    {
        return new Mock<T>(MockBehavior.Strict);
    }
}