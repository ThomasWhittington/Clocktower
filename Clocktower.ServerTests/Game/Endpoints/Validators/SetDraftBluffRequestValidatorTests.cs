using Clocktower.Server.Game.Endpoints;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Game.Endpoints.Validators;

[TestClass]
public class SetDraftBluffRequestValidatorTests
{
    private SetDraftBluff.RequestValidator _validator = null!;
    private const string ValidSnowflake = "123456789012345678";
    private const string RoleId = "role-id";

    [TestInitialize]
    public void Setup()
    {
        _validator = new SetDraftBluff.RequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldNotHaveErrors_WhenRequestIsValid()
    {
        var request = new SetDraftBluff.Request("gameId", ValidSnowflake, 1, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Validate_ShouldNotHaveErrors_WhenRoleIdIsNull()
    {
        var request = new SetDraftBluff.Request("gameId", ValidSnowflake, 1, null);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region GameId Tests

    [TestMethod]
    [DataRow("ab")]
    [DataRow("")]
    public void Validate_ShouldHaveError_WhenGameIdIsTooShort(string invalidGameId)
    {
        var request = new SetDraftBluff.Request(invalidGameId, ValidSnowflake, 1, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be less than 3 characters");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenGameIdIsTooLong()
    {
        var longGameId = new string('a', 33);
        var request = new SetDraftBluff.Request(longGameId, ValidSnowflake, 1, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be longer than 32 characters");
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenGameIdHasWhitespaceButTrimsToValidLength()
    {
        var request = new SetDraftBluff.Request("  abc  ", ValidSnowflake, 1, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.GameId);
    }

    #endregion

    #region UserId Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        var request = new SetDraftBluff.Request("valid-game", "", 1, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsNotSnowflake()
    {
        var request = new SetDraftBluff.Request("valid-game", "invalid-user", 1, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId must be a valid Discord snowflake");
    }

    #endregion

    #region Slot Tests

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    public void Validate_ShouldNotHaveError_WhenSlotIsInRange(int slot)
    {
        var request = new SetDraftBluff.Request("valid-game", ValidSnowflake, slot, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Slot);
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenSlotIsTooLow()
    {
        var request = new SetDraftBluff.Request("valid-game", ValidSnowflake, 0, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Slot);
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenSlotIsTooHigh()
    {
        var request = new SetDraftBluff.Request("valid-game", ValidSnowflake, 4, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Slot);
    }

    #endregion
}