using Clocktower.Server.Discord.Town.Endpoints.Validation;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Discord.Town.Endpoints.Validators;

[TestClass]
public class GuildIdRequestValidatorTests
{
    private GuildIdRequestValidator _validator = null!;

    [TestInitialize]
    public void Setup()
    {
        _validator = new GuildIdRequestValidator();
    }


    [TestMethod]
    public void Validate_ShouldHaveError_WhenGuildIdInvalid()
    {
        var request = new GuildIdRequest("invalid");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GuildId);
    }

    [TestMethod]
    public void Validate_ShouldNotHaveError_WhenGuildIdValid()
    {
        var request = new GuildIdRequest(CommonMethods.GetRandomSnowflakeStringId());

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}