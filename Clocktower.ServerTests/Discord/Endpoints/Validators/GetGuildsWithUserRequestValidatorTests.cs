using Clocktower.Server.Discord.Endpoints;
using FluentValidation.TestHelper;
using Request = Clocktower.Server.Discord.Endpoints.GetGuildsWithUser.Request;

namespace Clocktower.ServerTests.Discord.Endpoints.Validators;

[TestClass]
public class GetGuildsWithUserRequestValidatorTests
{
    private GetGuildsWithUser.RequestValidator _validator = null!;
    private const string ValidSnowflake = "123456789012345678";

    [TestInitialize]
    public void Setup()
    {
        _validator = new GetGuildsWithUser.RequestValidator();
    }


    [TestMethod]
    public void UserId_WhenEmpty_ShouldHaveValidationError()
    {
        var request = new Request("");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void UserId_WhenNull_ShouldHaveValidationError()
    {
        var request = new Request(null!);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void UserId_WhenWhitespace_ShouldHaveValidationError()
    {
        var request = new Request("   ");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void UserId_WhenInvalidSnowflake_ShouldHaveValidationError()
    {
        var request = new Request("invalid_snowflake");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId must be a valid Discord snowflake");
    }

    [TestMethod]
    public void UserId_WhenValidSnowflake_ShouldNotHaveValidationError()
    {
        var request = new Request(ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [TestMethod]
    public void Validate_WhenAllFieldsValid_ShouldPassValidation()
    {
        var request = new Request("123456789012345678");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}