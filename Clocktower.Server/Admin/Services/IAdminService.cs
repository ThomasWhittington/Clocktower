namespace Clocktower.Server.Admin.Services;

public interface IAdminService
{
    (bool succes, string result) GenerateJwtToken(string username);
    Task<Result<string>> ForceUpdate(string gameId);
}