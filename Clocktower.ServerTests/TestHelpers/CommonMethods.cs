global using Clocktower.Server.Common.Types;
using Clocktower.Server.Common;
using Clocktower.Server.Data;
using Microsoft.Extensions.Options;

namespace Clocktower.ServerTests.TestHelpers;

public static class CommonMethods
{
    public static string GetRandomString() => Guid.NewGuid().ToString();
    public static string GetRandomSnowflakeStringId() => new Random((int)DateTime.Now.Ticks).NextInt64().ToString();
    public static GameUser GetRandomGameUser(string? id = null) => new(id ?? GetRandomSnowflakeStringId());
    public static TownUser GetRandomTownUser(string? id = null, string? name = null) => new(id ?? GetRandomSnowflakeStringId(), name ?? GetRandomString(), GetRandomString());


    public static Secrets SetUpMockSecrets(
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
        return secrets;
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

    public static DateTime TruncateToSeconds(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day,
            dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Utc);
    }

    public static GameState GetGameState(string? gameId = null, string? guildId = null, string? creatorId = null, GameUser? createdBy = null)
    {
        return new GameState(gameId ?? GetRandomString(), guildId ?? GetRandomSnowflakeStringId(), createdBy ?? GetRandomGameUser(creatorId), DateTime.UtcNow);
    }
}