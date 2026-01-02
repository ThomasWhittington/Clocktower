using Clocktower.Server.Data;
using Clocktower.Server.Data.Wrappers;

namespace Clocktower.ServerTests.TestHelpers;

public static class MockMaker
{
    public static IDiscordGuildUser CreateMockDiscordGuildUser(string id, string name = "", string avatar = "", bool isAdmin = false)
    {
        return Mock.Of<IDiscordGuildUser>(u =>
            u.Id == id &&
            u.IsAdministrator() == isAdmin &&
            u.AsGameUser() == new GameUser(id) &&
            u.AsTownUser() == new TownUser(id, name, avatar)
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