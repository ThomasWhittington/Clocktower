using Clocktower.Server.Game.Endpoints;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Game.Endpoints.Validators;

[TestClass]
public class SetDraftRolesRequestValidatorTests
{
    private SetDraftRoles.RequestValidator _validator = null!;
    private static SetDraftRoles.Body Body => new(new Dictionary<string, string> { { "user", "role" } });


    [TestInitialize]
    public void Setup()
    {
        _validator = new SetDraftRoles.RequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldNotHaveErrors_WhenRequestIsValid()
    {
        var request = new SetDraftRoles.Request("gameId", Body);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #region GameId Tests

    [TestMethod]
    [DataRow("ab")]
    [DataRow("")]
    public void Validate_ShouldHaveError_WhenGameIdIsTooShort(string invalidGameId)
    {
        var request = new SetDraftRoles.Request(invalidGameId, Body);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be less than 3 characters");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenGameIdIsTooLong()
    {
        var longGameId = new string('a', 33);
        var request = new SetDraftRoles.Request(longGameId, Body);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId)
            .WithErrorMessage("GameId cannot be longer than 32 characters");
    }

    [TestMethod]
    public void Validate_ShouldPass_WhenGameIdHasWhitespaceButTrimsToValidLength()
    {
        var request = new SetDraftRoles.Request("  abc  ", Body);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.GameId);
    }

    #endregion

    [TestMethod]
    public void Validate_ShouldHaveError_WhenPlayerRolesEmpty()
    {
        var request = new SetDraftRoles.Request("  abc  ", new SetDraftRoles.Body([]));

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Body.PlayerRoles)
            .WithErrorMessage("'Body Player Roles' must not be empty.");
    }
}