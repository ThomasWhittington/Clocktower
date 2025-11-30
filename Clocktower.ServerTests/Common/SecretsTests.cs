using Clocktower.Server.Common;

namespace Clocktower.ServerTests.Common;

[TestClass]
public class SecretsTests
{
    private static Secrets GetSut(
        string? discordBotToken = "discordBotToken",
        string? discordClientId = "discordClientId",
        string? discordClientSecret = "discordClientSecret",
        string? serverUri = "serverUri",
        string? feUri = "feUri",
        string? jwtSigningKey = "jwtSigningKey",
        string? jwtAudience = "jwtAudience"
    )
    {
        return CommonMethods.GetSecrets(
            discordBotToken,
            discordClientId,
            discordClientSecret,
            serverUri,
            feUri,
            jwtSigningKey,
            jwtAudience
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
    public void HasAllSecrets_ReturnsFalse_WhenDiscordBotTokenMissing(string? val)
    {
        var sut = GetSut(discordBotToken: val);

        var result = sut.HasAllSecrets();

        result.success.Should().BeFalse();
        result.message.Should().Be("Missing DiscordBotToken");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void HasAllSecrets_ReturnsFalse_WhenDiscordClientIdMissing(string? val)
    {
        var sut = GetSut(discordClientId: val);

        var result = sut.HasAllSecrets();

        result.success.Should().BeFalse();
        result.message.Should().Be("Missing DiscordClientId");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void HasAllSecrets_ReturnsFalse_WhenDiscordClientSecretMissing(string? val)
    {
        var sut = GetSut(discordClientSecret: val);

        var result = sut.HasAllSecrets();

        result.success.Should().BeFalse();
        result.message.Should().Be("Missing DiscordClientSecret");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void HasAllSecrets_ReturnsFalse_WhenServerUriMissing(string? val)
    {
        var sut = GetSut(serverUri: val);

        var result = sut.HasAllSecrets();

        result.success.Should().BeFalse();
        result.message.Should().Be("Missing ServerUri");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void HasAllSecrets_ReturnsFalse_WhenFeUriMissing(string? val)
    {
        var sut = GetSut(feUri: val);

        var result = sut.HasAllSecrets();

        result.success.Should().BeFalse();
        result.message.Should().Be("Missing FeUri");
    }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void HasAllSecrets_ReturnsFalse_WhenJwtInvalid(string? val)
    {
        var sut = GetSut(jwtAudience: val, jwtSigningKey: val);
        var result = sut.HasAllSecrets();

        result.success.Should().BeFalse();
    }
}