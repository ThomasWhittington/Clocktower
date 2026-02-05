namespace Clocktower.Server.Common.Services;

public class IdGenerator : IIdGenerator
{
    public string GenerateId()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 10).Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }
}