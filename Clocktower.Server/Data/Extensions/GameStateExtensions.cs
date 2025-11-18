namespace Clocktower.Server.Data.Extensions;

public static class GameStateExtensions
{
    extension(GameState gameState)
    {
        public bool IsUserOfType(string userId, UserType userType)
        {
            return gameState.GetUserType(userId) == userType;
        }

        public UserType GetUserType(string userId)
        {
            var user = gameState.GetUser(userId);
            if (user is null) return UserType.Unknown;
            return user.UserType;
        }

        public GameUser? GetUser(string userId)
        {
            return gameState.Users.FirstOrDefault(o => o.Id == userId);
        }
    }
}