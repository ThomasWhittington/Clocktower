using Clocktower.Server.Game.Endpoints;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Game.Endpoints.Validators;

[TestClass]
public class SetPlayerHasVoteTokenRequestValidatorTests
{
    private SetPlayerHasVoteToken.RequestValidator _validator = null!;
    private const string ValidSnowflake = "123456789012345678";

    public static bool RandomBool => new Random().Next(2) == 0;

    [TestInitialize]
    public void Setup()
    {
        _validator = new SetPlayerHasVoteToken.RequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldNotHaveErrors_WhenRequestIsValid()
    {
        var request = new SetPlayerHasVoteToken.Request("valid-game", CommonMethods.GetRandomSnowflakeStringId(), RandomBool);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region GameId Tests

    [TestMethod]
    [DataRow("ab")]
    [DataRow("")]
    public void Validate_ShouldHaveError_WhenGameIdIsTooShort(string invalidGameId)
    {
        var request = new SetPlayerHasVoteToken.Request(invalidGameId, ValidSnowflake, RandomBool);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be less than 3 characters");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenGameIdIsTooLong()
    {
        var longGameId = new string('a', 33);
        var request = new SetPlayerHasVoteToken.Request(longGameId, ValidSnowflake, RandomBool);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be longer than 32 characters");
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenGameIdHasWhitespaceButTrimsToValidLength()
    {
        var request = new SetPlayerHasVoteToken.Request("  abc  ", ValidSnowflake, RandomBool);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.GameId);
    }

    #endregion

    #region UserId1 Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserId1IsEmpty()
    {
        var request = new SetPlayerHasVoteToken.Request("valid-game", "", RandomBool);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserId1IsNotSnowflake()
    {
        var request = new SetPlayerHasVoteToken.Request("valid-game", "invalid-user", RandomBool);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId must be a valid Discord snowflake");
    }

    #endregion
}