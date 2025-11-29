namespace Clocktower.Server.Common;

[UsedImplicitly]
public class JwtSecrets
{
    [UsedImplicitly] public required string SigningKey { get; set; }
    [UsedImplicitly] public required string Audience { get; set; }

    public (bool success, string message) HasAllSecrets()
    {
        if (string.IsNullOrWhiteSpace(SigningKey)) return (false, $"Missing {nameof(SigningKey)}");
        if (string.IsNullOrWhiteSpace(Audience)) return (false, $"Missing {nameof(Audience)}");
        return (true, string.Empty);
    }
}