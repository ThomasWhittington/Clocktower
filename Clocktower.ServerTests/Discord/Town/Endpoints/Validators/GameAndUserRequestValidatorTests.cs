using Clocktower.Server.Discord.Town.Endpoints.Validation;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Discord.Town.Endpoints.Validators;

[TestClass]
public class GameAndUserRequestValidatorTests
{
    private GameAndUserRequestValidator _validator = null!;
    private const string ValidSnowflake = "123456789012345678";

    [TestInitialize]
    public void Setup()
    {
        _validator = new GameAndUserRequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenGameIdInvalid()
    {
        var request = new GameAndUserRequest("valid-game", ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region GameId Tests

    [TestMethod]
    [DataRow("ab")]
    [DataRow("")]
    public void Validate_ShouldHaveError_WhenGameIdIsTooShort(string invalidGameId)
    {
        var request = new GameAndUserRequest(invalidGameId, ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be less than 3 characters");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenGameIdIsTooLong()
    {
        var longGameId = new string('a', 33);
        var request = new GameAndUserRequest(longGameId, ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be longer than 32 characters");
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenGameIdHasWhitespaceButTrimsToValidLength()
    {
        var request = new GameAndUserRequest("  abc  ", ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.GameId);
    }

    #endregion

    #region UserId Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        var request = new GameAndUserRequest("valid-game", "");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsNotSnowflake()
    {
        var request = new GameAndUserRequest("valid-game", "invalid-user");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId must be a valid Discord snowflake");
    }

    #endregion
}