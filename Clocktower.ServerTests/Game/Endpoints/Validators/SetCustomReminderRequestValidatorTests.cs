using Clocktower.Server.Game.Endpoints;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Game.Endpoints.Validators;

[TestClass]
public class SetCustomReminderRequestValidatorTests
{
    private SetCustomReminder.RequestValidator _validator = null!;
    private const string ValidSnowflake = "123456789012345678";
    private const string ValidSnowflake2 = "876543210987654321";

    [TestInitialize]
    public void Setup()
    {
        _validator = new SetCustomReminder.RequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldNotHaveErrors_WhenRequestIsValid()
    {
        var request = new SetCustomReminder.Request("gameId", ValidSnowflake, ValidSnowflake2, new SetCustomReminder.Body("Poisoned by the Imp"));

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region GameId Tests

    [TestMethod]
    [DataRow("ab")]
    [DataRow("")]
    public void Validate_ShouldHaveError_WhenGameIdIsTooShort(string invalidGameId)
    {
        var request = new SetCustomReminder.Request(invalidGameId, ValidSnowflake, ValidSnowflake2, new SetCustomReminder.Body("text"));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be less than 3 characters");
    }

    #endregion

    #region UserId / TargetUserId Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        var request = new SetCustomReminder.Request("valid-game", "", ValidSnowflake2, new SetCustomReminder.Body("text"));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsNotSnowflake()
    {
        var request = new SetCustomReminder.Request("valid-game", "invalid-user", ValidSnowflake2, new SetCustomReminder.Body("text"));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId must be a valid Discord snowflake");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenTargetUserIdIsEmpty()
    {
        var request = new SetCustomReminder.Request("valid-game", ValidSnowflake, "", new SetCustomReminder.Body("text"));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TargetUserId)
            .WithErrorMessage("TargetUserId cannot be empty");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenTargetUserIdIsNotSnowflake()
    {
        var request = new SetCustomReminder.Request("valid-game", ValidSnowflake, "invalid-user", new SetCustomReminder.Body("text"));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TargetUserId)
            .WithErrorMessage("TargetUserId must be a valid Discord snowflake");
    }

    #endregion

    #region ReminderText Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenReminderTextIsEmpty()
    {
        var request = new SetCustomReminder.Request("valid-game", ValidSnowflake, ValidSnowflake2, new SetCustomReminder.Body(""));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Body.ReminderText");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenReminderTextIsTooLong()
    {
        var longText = new string('a', 41);
        var request = new SetCustomReminder.Request("valid-game", ValidSnowflake, ValidSnowflake2, new SetCustomReminder.Body(longText));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("Body.ReminderText")
            .WithErrorMessage("ReminderText cannot be longer than 40 characters");
    }

    [TestMethod]
    public void Validate_ShouldNotHaveError_WhenReminderTextIsMaxLength()
    {
        var maxLengthText = new string('a', 40);
        var request = new SetCustomReminder.Request("valid-game", ValidSnowflake, ValidSnowflake2, new SetCustomReminder.Body(maxLengthText));

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor("Body.ReminderText");
    }

    #endregion
}
