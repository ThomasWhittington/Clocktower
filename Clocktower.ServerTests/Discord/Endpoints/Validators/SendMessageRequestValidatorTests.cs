using Clocktower.Server.Discord.Endpoints;
using FluentValidation.TestHelper;
using Request = Clocktower.Server.Discord.Endpoints.SendMessage.Request;

namespace Clocktower.ServerTests.Discord.Endpoints.Validators;

[TestClass]
public class SendMessageRequestValidatorTests
{
    private SendMessage.RequestValidator _validator = null!;
    private const string ValidSnowflake = "123456789012345678";

    [TestInitialize]
    public void Setup()
    {
        _validator = new SendMessage.RequestValidator();
    }

    [TestMethod]
    public void UserId_WhenEmpty_ShouldHaveValidationError()
    {
        var request = new Request("", "Test message");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void UserId_WhenNull_ShouldHaveValidationError()
    {
        var request = new Request(null!, "Test message");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void UserId_WhenWhitespace_ShouldHaveValidationError()
    {
        var request = new Request("   ", "Test message");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void UserId_WhenInvalidSnowflake_ShouldHaveValidationError()
    {
        var request = new Request("invalid_snowflake", "Test message");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId must be a valid Discord snowflake");
    }

    [TestMethod]
    public void UserId_WhenValidSnowflake_ShouldNotHaveValidationError()
    {
        var request = new Request(ValidSnowflake, "Test message");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [TestMethod]
    public void Message_WhenEmpty_ShouldHaveValidationError()
    {
        var request = new Request(ValidSnowflake, "");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Message)
            .WithErrorMessage("Message is required");
    }

    [TestMethod]
    public void Message_WhenNull_ShouldHaveValidationError()
    {
        var request = new Request(ValidSnowflake, null!);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Message)
            .WithErrorMessage("Message is required");
    }

    [TestMethod]
    public void Message_WhenWhitespace_ShouldHaveValidationError()
    {
        var request = new Request(ValidSnowflake, "   ");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Message)
            .WithErrorMessage("Message is required");
    }

    [TestMethod]
    public void Message_WhenValid_ShouldNotHaveValidationError()
    {
        var request = new Request(ValidSnowflake, "Hello, World!");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Message);
    }

    [TestMethod]
    public void Validate_WhenAllFieldsValid_ShouldPassValidation()
    {
        var request = new Request(ValidSnowflake, "Test message");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Validate_WhenAllFieldsInvalid_ShouldHaveMultipleErrors()
    {
        var request = new Request("", "");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
        result.ShouldHaveValidationErrorFor(x => x.Message);
        result.Errors.Should().HaveCount(3);
    }
}