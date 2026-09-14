using Clocktower.Server.Game.Endpoints;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Game.Endpoints.Validators;

[TestClass]
public class RemoveReminderRequestValidatorTests
{
    private RemoveReminder.RequestValidator _validator = null!;
    private const string ValidSnowflake = "123456789012345678";

    [TestInitialize]
    public void Setup()
    {
        _validator = new RemoveReminder.RequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldNotHaveErrors_WhenRequestIsValid()
    {
        var request = new RemoveReminder.Request("gameId", ValidSnowflake, "role-reminder");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region GameId Tests

    [TestMethod]
    [DataRow("ab")]
    [DataRow("")]
    public void Validate_ShouldHaveError_WhenGameIdIsTooShort(string invalidGameId)
    {
        var request = new RemoveReminder.Request(invalidGameId, ValidSnowflake, "role-reminder");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be less than 3 characters");
    }

    #endregion

    #region TargetUserId Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenTargetUserIdIsEmpty()
    {
        var request = new RemoveReminder.Request("valid-game", "", "role-reminder");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TargetUserId)
            .WithErrorMessage("TargetUserId cannot be empty");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenTargetUserIdIsNotSnowflake()
    {
        var request = new RemoveReminder.Request("valid-game", "invalid-user", "role-reminder");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TargetUserId)
            .WithErrorMessage("TargetUserId must be a valid Discord snowflake");
    }

    #endregion

    #region ReminderId Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenReminderIdIsEmpty()
    {
        var request = new RemoveReminder.Request("valid-game", ValidSnowflake, "");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ReminderId);
    }

    #endregion
}
