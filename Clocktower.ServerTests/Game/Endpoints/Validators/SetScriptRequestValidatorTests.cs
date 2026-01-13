using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Game.Endpoints;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Game.Endpoints.Validators;

[TestClass]
public class SetScriptRequestValidatorTests
{
    private SetScript.RequestValidator _validator = null!;

    [TestInitialize]
    public void Setup()
    {
        _validator = new SetScript.RequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldNotHaveErrors_WhenRequestIsValid_Custom()
    {
        var request = new SetScript.Request("gameId", ScriptSelect.Custom, "{}");

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Validate_ShouldNotHaveErrors_WhenRequestIsValid_Predefined()
    {
        var request = new SetScript.Request("gameId", ScriptSelect.SectsAndViolets, null);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenJsonIsInvalid()
    {
        var request = new SetScript.Request("gameId", ScriptSelect.Custom, "invalid");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Json).WithErrorMessage("Json must be valid JSON");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenJsonMissing()
    {
        var request = new SetScript.Request("gameId", ScriptSelect.Custom, null);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Json)
            .When(e => e.ErrorMessage == "Json must be present if ScriptSelect is Custom");
        result.ShouldHaveValidationErrorFor(x => x.Json)
            .When(e => e.ErrorMessage == "Json must be valid JSON");
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenJsonNotNeeded()
    {
        var request = new SetScript.Request("gameId", ScriptSelect.SectsAndViolets, "{}");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Json)
            .When(e => e.ErrorMessage == "Json must be empty when ScriptSelect is not Custom");
    }
}