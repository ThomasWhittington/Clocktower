using Clocktower.Server.Data;
using Clocktower.Server.Data.Wrappers;

namespace Clocktower.ServerTests.TestHelpers;

public static class MockMaker
{
    public static IDiscordUser CreateMockDiscordUser(string id, string globalName, string avatarUrl)
    {
        var mockUser = Mock.Of<IDiscordUser>(u =>
            u.Id == id &&
            u.GlobalName == globalName &&
            u.DisplayAvatarUrl == avatarUrl &&
            u.AsGameUser() == new GameUser(id.ToString()));
        return mockUser;
    }

    public static IDiscordGuildUser CreateMockDiscordGuildUser(string id, bool isAdmin = false)
    {
        return Mock.Of<IDiscordGuildUser>(u =>
            u.Id == id &&
            u.IsAdministrator() == isAdmin
        );
    }

    public static IDiscordGuild CreateMockDiscordGuild(string id, string name, IEnumerable<IDiscordGuildUser>? users = null)
    {
        users ??= [];
        return Mock.Of<IDiscordGuild>(g =>
            g.Id == id &&
            g.Name == name &&
            g.Users == users);
    }
}