using Clocktower.Server.Data;
using Clocktower.Server.Data.Dto;
using Clocktower.Server.Data.Types.Enum;

namespace Clocktower.ServerTests.Data;

[TestClass]
public class DiscordTownDtoTests
{
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public void DefaultRoleDistribution_LessThan5Players_BeNull(int playerCount)
    {
        var perspective = GetDiscordTownDto(playerCount);

        perspective.DefaultRoleDistributions.Should().BeNull();
    }

    [TestMethod]
    [DataRow(5, 3, 0, 1, 1)]
    [DataRow(6, 3, 1, 1, 1)]
    [DataRow(7, 5, 0, 1, 1)]
    [DataRow(8, 5, 1, 1, 1)]
    [DataRow(9, 5, 2, 1, 1)]
    [DataRow(10, 7, 0, 2, 1)]
    [DataRow(11, 7, 1, 2, 1)]
    [DataRow(12, 7, 2, 2, 1)]
    [DataRow(13, 9, 0, 3, 1)]
    [DataRow(14, 9, 1, 3, 1)]
    [DataRow(15, 9, 2, 3, 1)]
    public void DefaultRoleDistribution_ValidPlayerCounts_CorrectDistribution(
        int playerCount, int townsfolk, int outsiders, int minions, int demons)
    {
        AssertDefaultRoleDistribution(playerCount, new RoleDistribution(townsfolk, outsiders, minions, demons));
    }

    private static DiscordTownDto GetDiscordTownDto(int playerCount)
    {
        var players = new List<UserDto>();

        for (int i = 0; i < playerCount; i++)
        {
            players.Add(new UserDto(CommonMethods.GetRandomSnowflakeStringId(), CommonMethods.GetRandomString(), CommonMethods.GetRandomString())
                { UserType = UserType.Player });
        }

        return new DiscordTownDto("game-id", []) { GameUsers = players };
    }

    private static void AssertDefaultRoleDistribution(int playerCount, RoleDistribution expectedDistribution)
    {
        var roleDistributionTotal = expectedDistribution.Townsfolk + expectedDistribution.Outsiders + expectedDistribution.Minions + expectedDistribution.Demons;
        roleDistributionTotal.Should().Be(playerCount);

        var dto = GetDiscordTownDto(playerCount);

        dto.DefaultRoleDistributions.Should().NotBeNull();
        dto.DefaultRoleDistributions.Townsfolk.Should().Be(expectedDistribution.Townsfolk);
        dto.DefaultRoleDistributions.Outsiders.Should().Be(expectedDistribution.Outsiders);
        dto.DefaultRoleDistributions.Minions.Should().Be(expectedDistribution.Minions);
        dto.DefaultRoleDistributions.Demons.Should().Be(expectedDistribution.Demons);
    }
}