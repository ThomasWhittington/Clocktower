using Clocktower.Server.Common;

namespace Clocktower.ServerTests.Common;

[TestClass]
public class JwtSecretsTests
{
    private static JwtSecrets GetSut(
        string? signingKey = "signingKey",
        string? audience = "audience"
    )
    {
        return CommonMethods.GetJwtSecrets(
            signingKey,
            audience
        );
    }

    [TestMethod]
    public void HasAllSecrets_ReturnsTrue_WhenHasAllSecrets()
    {
        var sut = GetSut();

        var result = sut.HasAllSecrets();

        result.success.Should().BeTrue();
        result.message.Should().BeEmpty();
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void HasAllSecrets_ReturnsFalse_WhenSigningKeyMissing(string? val)
    {
        var sut = GetSut(signingKey: val);

        var result = sut.HasAllSecrets();

        result.success.Should().BeFalse();
        result.message.Should().Be("Missing SigningKey");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void HasAllSecrets_ReturnsFalse_WhenAudienceMissing(string? val)
    {
        var sut = GetSut(audience: val);

        var result = sut.HasAllSecrets();

        result.success.Should().BeFalse();
        result.message.Should().Be("Missing Audience");
    }
}