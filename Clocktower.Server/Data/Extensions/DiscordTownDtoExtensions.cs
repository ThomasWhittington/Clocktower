namespace Clocktower.Server.Data.Extensions;

public static class Calculator
{
    public static RoleDistribution? GetDefaultRoleDistribution(int playerCount)
    {
        if (playerCount < 5) return null;

        const int demons = 1;
        int minions = (playerCount - 7) / 3 + 1;
        int outsiders = playerCount switch
        {
            5 => 0,
            6 => 1,
            _ => (playerCount - 7) % 3
        };
        int townsfolk = playerCount - outsiders - minions - demons;

        return new RoleDistribution(townsfolk, outsiders, minions, demons);
    }
}