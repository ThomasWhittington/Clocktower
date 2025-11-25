using Clocktower.Server.Discord.Town.Endpoints.Validation;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Discord.Town.Endpoints.Validators;

[TestClass]
public class UserIdRequestValidatorTests
{
    private UserIdRequestValidator _validator = null!;
    private const string ValidSnowflake = "123456789012345678";

    [TestInitialize]
    public void Setup()
    {
        _validator = new UserIdRequestValidator();
    }


    [TestMethod]
    public void UserId_WhenEmpty_ShouldHaveValidationError()
    {
        var request = new UserIdRequest("");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void UserId_WhenNull_ShouldHaveValidationError()
    {
        var request = new UserIdRequest(null!);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void UserId_WhenWhitespace_ShouldHaveValidationError()
    {
        var request = new UserIdRequest("   ");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void UserId_WhenInvalidSnowflake_ShouldHaveValidationError()
    {
        var request = new UserIdRequest("invalid_snowflake");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId must be a valid Discord snowflake");
    }

    [TestMethod]
    public void UserId_WhenValidSnowflake_ShouldNotHaveValidationError()
    {
        var request = new UserIdRequest(ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [TestMethod]
    public void Validate_WhenAllFieldsValid_ShouldPassValidation()
    {
        var request = new UserIdRequest("123456789012345678");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}