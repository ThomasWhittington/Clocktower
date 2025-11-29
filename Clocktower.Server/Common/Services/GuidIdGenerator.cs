namespace Clocktower.Server.Common.Services;

public class GuidIdGenerator : IIdGenerator
{
    public string GenerateId() => Guid.NewGuid().ToString();
}