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
        var request = new StartGame.Request(ValidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region GuildId Tests

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void Validate_ShouldHaveError_WhenGuildIdIsEmpty(string? guildId)
    {
        var request = new StartGame.Request(guildId!);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GuildId)
            .WithErrorMessage("GuildId cannot be empty");
    }

    [TestMethod]
    [DataRow("not-a-snowflake")]
    [DataRow("123")]
    public void Validate_ShouldHaveError_WhenGuildIdIsNotSnowflake(string invalidSnowflake)
    {
        var request = new StartGame.Request(invalidSnowflake);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GuildId)
            .WithErrorMessage("GuildId must be a valid Discord snowflake");
    }

    #endregion
}
