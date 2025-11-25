using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Discord.Town.Endpoints;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Discord.Town.Endpoints.Validators;
[TestClass]
public class SetTimeRequestValidatorTests
{
    private SetTime.RequestValidator _validator = null!;
    
    [TestInitialize]
    public void Setup()
    {
        _validator = new SetTime.RequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenGameIdTooShort()
    {
        var request = new SetTime.Request("a", GameTime.Day);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GameId);
    }
}