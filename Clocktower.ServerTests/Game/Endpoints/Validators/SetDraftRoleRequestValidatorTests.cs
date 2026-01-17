using Clocktower.Server.Game.Endpoints;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Game.Endpoints.Validators;

[TestClass]
public class SetDraftRoleRequestValidatorTests
{
    private SetDraftRole.RequestValidator _validator = null!;
    private const string ValidSnowflake = "123456789012345678";
    private const string RoleId = "role-id";

    [TestInitialize]
    public void Setup()
    {
        _validator = new SetDraftRole.RequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldNotHaveErrors_WhenRequestIsValid()
    {
        var request = new SetDraftRole.Request("gameId", ValidSnowflake, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region GameId Tests

    [TestMethod]
    [DataRow("ab")]
    [DataRow("")]
    public void Validate_ShouldHaveError_WhenGameIdIsTooShort(string invalidGameId)
    {
        var request = new SetDraftRole.Request(invalidGameId, ValidSnowflake, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be less than 3 characters");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenGameIdIsTooLong()
    {
        var longGameId = new string('a', 33);
        var request = new SetDraftRole.Request(longGameId, ValidSnowflake, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be longer than 32 characters");
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenGameIdHasWhitespaceButTrimsToValidLength()
    {
        var request = new SetDraftRole.Request("  abc  ", ValidSnowflake, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.GameId);
    }

    #endregion

    #region TargetUserId Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        var request = new SetDraftRole.Request("valid-game", "", RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TargetUserId)
            .WithErrorMessage("TargetUserId cannot be empty");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsNotSnowflake()
    {
        var request = new SetDraftRole.Request("valid-game", "invalid-user", RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TargetUserId)
            .WithErrorMessage("TargetUserId must be a valid Discord snowflake");
    }

    #endregion
}