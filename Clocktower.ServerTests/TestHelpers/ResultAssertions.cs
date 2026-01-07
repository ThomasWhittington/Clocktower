namespace Clocktower.ServerTests.TestHelpers;

public static class ResultAssertions
{
    extension<T>(Result<T> result)
    {
        public void ShouldFailWith(ErrorKind kind, string code, string? message = null)
        {
            result.Error.Should().NotBeNull();
            if (message is not null)
                result.Error.Message.Should().Be(message);
            result.Error.Kind.Should().Be(kind);
            result.Error.Code.Should().Be(code);
            result.IsSuccess.Should().BeFalse();
        }

        public void ShouldSucceedWith(T expectedValue)
        {
            result.Error.Should().BeNull();
            result.Value.Should().Be(expectedValue);
            result.IsSuccess.Should().BeTrue();
        }
    }

    extension<T>(ErrorResponse? error)
    {
        public void ShouldBeError(Result<T> result)
        {
            error.Should().NotBeNull();
            error.Code.Should().Be(result.Error!.Code);
            error.Message.Should().Be(result.Error!.Message);
        }
    }
}