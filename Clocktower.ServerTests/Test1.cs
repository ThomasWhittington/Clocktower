using FluentAssertions;

namespace Clocktower.ServerTests;

[TestClass]
public sealed class Test1
{
    [TestMethod]
    public void TestMethod1()
    {
        false.Should().BeTrue();
    }
}