using Clocktower.Server.Game.Endpoints;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Game.Endpoints.Validators;

[TestClass]
public class SwapSeatingPositionsRequestValidatorTests
{
    private SwapSeatingPositions.RequestValidator _validator = null!;
    private const string ValidSnowflake = "123456789012345678";

    [TestInitialize]
    public void Setup()
    {
        _validator = new SwapSeatingPositions.RequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldNotHaveErrors_WhenRequestIsValid()
    {
        var request = new SwapSeatingPositions.Request("valid-game", ValidSnowflake, ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region GameId Tests

    [TestMethod]
    [DataRow("ab")]
    [DataRow("")]
    public void Validate_ShouldHaveError_WhenGameIdIsTooShort(string invalidGameId)
    {
        var request = new SwapSeatingPositions.Request(invalidGameId, ValidSnowflake, ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be less than 3 characters");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenGameIdIsTooLong()
    {
        var longGameId = new string('a', 33);
        var request = new SwapSeatingPositions.Request(longGameId, ValidSnowflake, ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be longer than 32 characters");
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenGameIdHasWhitespaceButTrimsToValidLength()
    {
        var request = new SwapSeatingPositions.Request("  abc  ", ValidSnowflake, ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.GameId);
    }

    #endregion

    #region UserId1 Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserId1IsEmpty()
    {
        var request = new SwapSeatingPositions.Request("valid-game", "", ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId1)
            .WithErrorMessage("UserId1 cannot be empty");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserId1IsNotSnowflake()
    {
        var request = new SwapSeatingPositions.Request("valid-game", "invalid-user", ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId1)
            .WithErrorMessage("UserId1 must be a valid Discord snowflake");
    }

    #endregion

    #region UserId2 Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserId2IsEmpty()
    {
        var request = new SwapSeatingPositions.Request("valid-game", ValidSnowflake, "");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId2)
            .WithErrorMessage("UserId2 cannot be empty");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserId2IsNotSnowflake()
    {
        var request = new SwapSeatingPositions.Request("valid-game", ValidSnowflake, "invalid-user");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId2)
            .WithErrorMessage("UserId2 must be a valid Discord snowflake");
    }

    #endregion
}