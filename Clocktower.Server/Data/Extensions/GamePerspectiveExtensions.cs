namespace Clocktower.Server.Data.Extensions;

public static class GamePerspectiveExtensions
{
    extension(GamePerspective gamePerspective)
    {
        public bool IsUserOfType(string userId, UserType userType)
        {
            return gamePerspective.GetUserType(userId) == userType;
        }

        public UserType GetUserType(string userId)
        {
            var user = gamePerspective.GetUser(userId);
            if (user is null) return UserType.Unknown;
            return user.UserType;
        }

        public GameUser? GetUser(string userId)
        {
            return gamePerspective.Users.FirstOrDefault(o => o.Id == userId);
        }
    }
}