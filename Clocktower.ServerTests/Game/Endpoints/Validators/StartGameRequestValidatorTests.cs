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
        var request = new StartGame.Request(ValidSnowflake, ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region GuildId Tests

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void Validate_ShouldHaveError_WhenGuildIdIsEmpty(string? guildId)
    {
        var request = new StartGame.Request(guildId!, ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GuildId)
            .WithErrorMessage("GuildId cannot be empty");
    }

    [TestMethod]
    [DataRow("not-a-snowflake")]
    [DataRow("123")]
    public void Validate_ShouldHaveError_WhenGuildIdIsNotSnowflake(string invalidSnowflake)
    {
        var request = new StartGame.Request(invalidSnowflake, ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GuildId)
            .WithErrorMessage("GuildId must be a valid Discord snowflake");
    }

    #endregion

    #region UserId Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        var request = new StartGame.Request("123456789012345678", "");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsNotSnowflake()
    {
        var request = new StartGame.Request("123456789012345678", "invalid-user");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId must be a valid Discord snowflake");
    }

    #endregion
}