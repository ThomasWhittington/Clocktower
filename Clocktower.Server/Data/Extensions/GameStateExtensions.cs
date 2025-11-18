namespace Clocktower.Server.Data.Extensions;

public static class GameStateExtensions
{
    public static bool IsUserOfType(this GameState gameState, string userId, UserType userType)
    {
        return gameState.GetUserType(userId) == userType;
    }

    public static UserType GetUserType(this GameState gameState, string userId)
    {
        var user = gameState.GetUser(userId);
        if (user is null) return UserType.Unknown;
        return user.UserType;
    }

    public static GameUser? GetUser(this GameState gameState, string userId)
    {
        return gameState.Users.FirstOrDefault(o => o.Id == userId);
    }
}