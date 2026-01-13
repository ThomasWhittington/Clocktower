namespace Clocktower.Server.Common.Services;

public interface IScriptProvider
{
    Task<Result<Script>> GetScriptAsync(ScriptSelect scriptSelect, string? json);
}