using Clocktower.Server.Game.Endpoints;
using Clocktower.Server.Timer.Endpoints;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Game.Endpoints.Validators;

[TestClass]
public class StartOrEditTimerRequestValidatorTests
{
    private StartOrEditTimer.RequestValidator _validator = null!;

    [TestInitialize]
    public void Setup()
    {
        _validator = new StartOrEditTimer.RequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldNotHaveErrors_WhenRequestIsValid()
    {
        var request = new StartOrEditTimer.Request(10, null);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [TestMethod]
    public void Validate_ShouldHaveErrors_WhenDurationIs0()
    {
        var request = new StartOrEditTimer.Request(0, null);
        
        var result = _validator.TestValidate(request);
        
        result.ShouldHaveValidationErrorFor(x => x.DurationSeconds)
            .WithErrorMessage("'Duration Seconds' must be greater than '0'.");
    }
}