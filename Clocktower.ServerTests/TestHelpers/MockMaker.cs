using Discord;

namespace Clocktower.ServerTests.TestHelpers;

public static class MockMaker
{
    public static IUser CreateMockDiscordUser(ulong id, string globalName, string avatarUrl)
    {
        return Mock.Of<IUser>(u => 
            u.Id == id && 
            u.GlobalName == globalName && 
            u.GetDisplayAvatarUrl() == avatarUrl);
    }

}