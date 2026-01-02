using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Discord.Town.Endpoints;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Discord.Town.Endpoints.Validators;

[TestClass]
public class SetUserTypeRequestValidatorTests
{
    private SetUserType.RequestValidator _validator = null!;
    private const string ValidSnowflake = "123456789012345678";

    [TestInitialize]
    public void Setup()
    {
        _validator = new SetUserType.RequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldNotHaveError_WhenRequestOk()
    {
        var request = new SetUserType.Request("valid-game", ValidSnowflake, UserType.Player);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region GameId Tests

    [TestMethod]
    [DataRow("ab")]
    [DataRow("")]
    public void Validate_ShouldHaveError_WhenGameIdIsTooShort(string invalidGameId)
    {
        var request = new SetUserType.Request(invalidGameId, ValidSnowflake, UserType.Player);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be less than 3 characters");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenGameIdIsTooLong()
    {
        var longGameId = new string('a', 33);
        var request = new SetUserType.Request(longGameId, ValidSnowflake, UserType.Player);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be longer than 32 characters");
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenGameIdHasWhitespaceButTrimsToValidLength()
    {
        var request = new SetUserType.Request("  abc  ", ValidSnowflake, UserType.Player);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.GameId);
    }

    #endregion

    #region UserId Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        var request = new SetUserType.Request("valid-game", "", UserType.Player);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsNotSnowflake()
    {
        var request = new SetUserType.Request("valid-game", "invalid-user", UserType.Player);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId must be a valid Discord snowflake");
    }

    #endregion

    #region UserType Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserTypeIsUnknown()
    {
        var request = new SetUserType.Request("valid-game", ValidSnowflake, UserType.Unknown);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserType)
            .WithErrorMessage("'User Type' must not be equal to 'Unknown'.");
    }

    #endregion
}