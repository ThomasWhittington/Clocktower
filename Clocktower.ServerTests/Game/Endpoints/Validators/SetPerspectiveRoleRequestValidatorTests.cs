using Clocktower.Server.Game.Endpoints;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Game.Endpoints.Validators;

[TestClass]
public class SetPerspectiveRoleRequestValidatorTests
{
    private SetPerspectiveRole.RequestValidator _validator = null!;
    private const string ValidSnowflake = "123456789012345678";
    private const string RoleId = "role-id";

    [TestInitialize]
    public void Setup()
    {
        _validator = new SetPerspectiveRole.RequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldNotHaveErrors_WhenRequestIsValid()
    {
        var request = new SetPerspectiveRole.Request("gameId", ValidSnowflake, ValidSnowflake, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region GameId Tests

    [TestMethod]
    [DataRow("ab")]
    [DataRow("")]
    public void Validate_ShouldHaveError_WhenGameIdIsTooShort(string invalidGameId)
    {
        var request = new SetPerspectiveRole.Request(invalidGameId, ValidSnowflake, ValidSnowflake, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be less than 3 characters");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenGameIdIsTooLong()
    {
        var longGameId = new string('a', 33);
        var request = new SetPerspectiveRole.Request(longGameId, ValidSnowflake, ValidSnowflake, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be longer than 32 characters");
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenGameIdHasWhitespaceButTrimsToValidLength()
    {
        var request = new SetPerspectiveRole.Request("  abc  ", ValidSnowflake, ValidSnowflake, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.GameId);
    }

    #endregion

    #region UserId Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        var request = new SetPerspectiveRole.Request("valid-game", "", ValidSnowflake, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId cannot be empty");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdIsNotSnowflake()
    {
        var request = new SetPerspectiveRole.Request("valid-game", "invalid-user", ValidSnowflake, RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId must be a valid Discord snowflake");
    }

    #endregion

    #region TargetUserId Tests

    [TestMethod]
    public void Validate_ShouldHaveError_WhenTargetUserIdIsEmpty()
    {
        var request = new SetPerspectiveRole.Request("valid-game", ValidSnowflake, "", RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TargetUserId)
            .WithErrorMessage("TargetUserId cannot be empty");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenTargetUserIdIsNotSnowflake()
    {
        var request = new SetPerspectiveRole.Request("valid-game", ValidSnowflake, "invalid-user", RoleId);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TargetUserId)
            .WithErrorMessage("TargetUserId must be a valid Discord snowflake");
    }

    #endregion
}