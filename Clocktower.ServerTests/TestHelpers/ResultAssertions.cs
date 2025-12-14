namespace Clocktower.ServerTests.TestHelpers;

public static class ResultAssertions
{
    extension(Result<string> result)
    {
        public void ShouldFailWith(ErrorKind kind, string code, string? message = null)
        {
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Kind.Should().Be(kind);
            result.Error.Code.Should().Be(code);

            if (message is not null)
                result.Error.Message.Should().Be(message);
        }

        public void ShouldSucceedWith(string expectedValue)
        {
            result.IsSuccess.Should().BeTrue();
            result.Error.Should().BeNull();
            result.Value.Should().Be(expectedValue);
        }
    }

    extension(ErrorResponse? error)
    {
        public void ShouldBeError(Result<string> result)
        {
            error.Should().NotBeNull();
            error.Code.Should().Be(result.Error!.Code);
            error.Message.Should().Be(result.Error!.Message);
        }
    }
}