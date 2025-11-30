using Clocktower.Server.Game.Endpoints;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Game.Endpoints.Validators;

[TestClass]
public class StartGameRequestValidatorTests
{
    private StartGame.RequestValidator _validator = null!;
    private const string ValidSnowflake = "123456789012345678";

    [TestInitialize]
    public void Setup()
    {
        _validator = new StartGame.RequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldNotHaveErrors_WhenRequestIsValid()
    {
        var request = new StartGame.Request("valid-game", ValidSnowflake, ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region GameId Tests

    [TestMethod]
    [DataRow("ab")]
    [DataRow("")]
    public void Validate_ShouldHaveError_WhenGameIdIsTooShort(string invalidGameId)
    {
        var request = new StartGame.Request(invalidGameId, ValidSnowflake, ValidSnowflake);
        
        var result = _validator.TestValidate(request);
        
        result.ShouldHaveValidationErrorFor(x => x.GameId)
              .WithErrorMessage("GameId cannot be less than 3 characters");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenGameIdIsTooLong()
    {
        var longGameId = new string('a', 33);
        var request = new StartGame.Request(longGameId, ValidSnowflake, ValidSnowflake);
        
        var result = _validator.TestValidate(request);
        
        result.ShouldHaveValidationErrorFor(x => x.GameId)
              .WithErrorMessage("GameId cannot be longer than 32 characters");
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenGameIdHasWhitespaceButTrimsToValidLength()
    {
        var request = new StartGame.Request("  abc  ", ValidSnowflake, ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.GameId);
    }

    #endregion

    #region GuildId Tests

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void Validate_ShouldHaveError_WhenGuildIdIsEmpty(string? guildId)
    {
        var request = new StartGame.Request("valid-game", guildId!, ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GuildId)
              .WithErrorMessage("GuildId cannot be empty");
    }

    [TestMethod]
    [DataRow("not-a-snowflake")]
    [DataRow("123")] 
    public void Validate_ShouldHaveError_WhenGuildIdIsNotSnowflake(string invalidSnowflake)
    {
        var request = new StartGame.Request("valid-game", invalidSnowflake, ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GuildId)
              .WithErrorMessage("GuildId must be a valid Discord snowflake");
    }

    #endregion

    #region UserId Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        var request = new StartGame.Request("valid-game", "123456789012345678", "");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsNotSnowflake()
    {
        var request = new StartGame.Request("valid-game", "123456789012345678", "invalid-user");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
              .WithErrorMessage("UserId must be a valid Discord snowflake");
    }

    #endregion
}