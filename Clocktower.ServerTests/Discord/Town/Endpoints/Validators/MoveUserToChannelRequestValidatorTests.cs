using Clocktower.Server.Discord.Town.Endpoints;
using FluentValidation.TestHelper;

namespace Clocktower.ServerTests.Discord.Town.Endpoints.Validators;

[TestClass]
public class MoveUserToChannelRequestValidatorTests
{
    private MoveUserToChannel.RequestValidator _validator = null!;

    [TestInitialize]
    public void Setup()
    {
        _validator = new MoveUserToChannel.RequestValidator();
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenGuildIdInvalid()
    {
        var request = new MoveUserToChannel.Request("invalid", CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomSnowflakeStringId());

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.GuildId);
    }

    [TestMethod]
    public void Validate_ShouldHaveError_WhenUserIdInvalid()
    {
        var request = new MoveUserToChannel.Request(CommonMethods.GetRandomSnowflakeStringId(), "invalid", CommonMethods.GetRandomSnowflakeStringId());

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }


    [TestMethod]
    public void Validate_ShouldHaveError_WhenChannelIdInvalid()
    {
        var request = new MoveUserToChannel.Request(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomSnowflakeStringId(), "invalid");

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ChannelId);
    }
    
    [TestMethod]
    public void Validate_ShouldNotHaveError_WhenAllValid()
    {
        var request = new MoveUserToChannel.Request(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomSnowflakeStringId());

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}