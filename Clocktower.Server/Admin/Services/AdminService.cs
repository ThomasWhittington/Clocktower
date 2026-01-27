using Clocktower.Server.Common.Services;
using Clocktower.Server.Socket.Services;

namespace Clocktower.Server.Admin.Services;

[UsedImplicitly]
public class AdminService(IJwtWriter jwtWriter, IGameBroadcastService gameBroadcastService) : IAdminService
{
    public (bool succes, string result) GenerateJwtToken(string username)
    {
        try
        {
            var gameUser = new GameUser("0")
            {
                UserType = UserType.StoryTeller
            };
            var jwt = jwtWriter.GetJwtToken(gameUser, isTest: true);
            return (true, jwt);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<Result<string>> ForceUpdate(string gameId)
    {
        await gameBroadcastService.BroadcastDiscordTownUpdate(gameId);
        return Result.Ok($"Update sent for game: '{gameId}'");
    }
}