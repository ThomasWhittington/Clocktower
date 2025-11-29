using Clocktower.Server.Common;
using Clocktower.Server.Data;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace Clocktower.ServerTests.TestHelpers;

public static class CommonMethods
{
    public static string GetRandomString() => Guid.NewGuid().ToString();
    public static string GetRandomSnowflakeStringId() => GetRandomSnowflakeNumberId().ToString();
    public static ulong GetRandomSnowflakeNumberId() => (ulong)new Random((int)DateTime.Now.Ticks).NextInt64();
    public static GameUser GetRandomGameUser() => new(GetRandomSnowflakeStringId(), GetRandomString(), GetRandomString());


    public static void SetUpMockSecrets(
        Mock<IOptions<Secrets>> mockSecrets,
        string? discordBotToken = null,
        string? discordClientId = null,
        string? discordClientSecret = null,
        string? serverUri = null,
        string? feUri = null,
        string? jwtSigningKey = null,
        string? jwtAudience = null
    )
    {
        var secrets = GetSecrets(
            discordBotToken,
            discordClientId,
            discordClientSecret,
            serverUri,
            feUri,
            jwtSigningKey,
            jwtAudience
        );
        mockSecrets.Setup(o => o.Value).Returns(secrets);
    }

    public static Secrets GetSecrets(
        string? discordBotToken = null,
        string? discordClientId = null,
        string? discordClientSecret = null,
        string? serverUri = null,
        string? feUri = null,
        string? jwtSigningKey = null,
        string? jwtAudience = null
    )
    {
        var secrets = new Secrets
        {
            DiscordBotToken = discordBotToken!,
            DiscordClientId = discordClientId!,
            DiscordClientSecret = discordClientSecret!,
            ServerUri = serverUri!,
            FeUri = feUri!,
            Jwt = GetJwtSecrets(
                signingKey: jwtSigningKey,
                audience: jwtAudience
            )
        };
        return secrets;
    }

    public static JwtSecrets GetJwtSecrets(
        string? signingKey = null,
        string? audience = null
    )
    {
        return new JwtSecrets
        {
            SigningKey = signingKey!,
            Audience = audience!
        };
    }
}