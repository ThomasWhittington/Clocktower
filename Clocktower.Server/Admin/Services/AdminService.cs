using Clocktower.Server.Common.Services;

namespace Clocktower.Server.Admin.Services;

[UsedImplicitly]
public class AdminService(IJwtWriter jwtWriter) : IAdminService
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
}