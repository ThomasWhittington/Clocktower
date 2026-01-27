namespace Clocktower.Server.Common.Services;

public class IdGenerator : IIdGenerator
{
    public string GenerateId() => Guid.NewGuid().ToString();

    public string GenerateGameId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 10)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}