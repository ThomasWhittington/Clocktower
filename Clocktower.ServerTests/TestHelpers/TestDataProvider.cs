using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Data.Types.Role;

namespace Clocktower.ServerTests.TestHelpers;

public static class TestDataProvider
{
    public static IEnumerable<object[]> GetAllEnumValues<T>() where T : struct, Enum
    {
        return Enum.GetValues<T>().Select(enumValue => new object[] { enumValue });
    }

    public static IEnumerable<object[]> GenerateBooleanCombinations(int count)
    {
        var totalCombinations = (int)Math.Pow(2, count);

        for (int i = 0; i < totalCombinations; i++)
        {
            var combination = new object[count];
            for (int j = 0; j < count; j++)
            {
                combination[j] = (i & (1 << j)) != 0;
            }

            yield return combination;
        }
    }

    public static IEnumerable<Role> GetDummyRoles()
    {
        return
        [
            new Role("tb-tf", "trouble_brewing-townsfolk", RoleType.Townsfolk, Edition.TroubleBrewing),
            new Role("tb-o", "trouble_brewing-outsider", RoleType.Outsider, Edition.TroubleBrewing),
            new Role("tb-m", "trouble_brewing-minion", RoleType.Minion, Edition.TroubleBrewing),
            new Role("tb-d", "trouble_brewing-demon", RoleType.Demon, Edition.TroubleBrewing),
            new Role("tb-t", "trouble_brewing-traveller", RoleType.Traveller, Edition.TroubleBrewing),
            new Role("sv-tf", "sects_and_violets-townsfolk", RoleType.Townsfolk, Edition.SectsAndViolets),
            new Role("sv-o", "sects_and_violets-outsider", RoleType.Outsider, Edition.SectsAndViolets),
            new Role("sv-m", "sects_and_violets-minion", RoleType.Minion, Edition.SectsAndViolets),
            new Role("sv-d", "sects_and_violets-demon", RoleType.Demon, Edition.SectsAndViolets),
            new Role("sv-t", "sects_and_violets-traveller", RoleType.Traveller, Edition.SectsAndViolets),
            new Role("bmr-tf", "bad_moon_rising-townsfolk", RoleType.Townsfolk, Edition.BadMoonRising),
            new Role("bmr-o", "bad_moon_rising-outsider", RoleType.Outsider, Edition.BadMoonRising),
            new Role("bmr-m", "bad_moon_rising-minion", RoleType.Minion, Edition.BadMoonRising),
            new Role("bmr-d", "bad_moon_rising-demon", RoleType.Demon, Edition.BadMoonRising),
            new Role("bmr-t", "bad_moon_rising-traveller", RoleType.Traveller, Edition.BadMoonRising),
            new Role("car-tf", "carousel-townsfolk", RoleType.Townsfolk, Edition.Carousel),
            new Role("car-o", "carousel-outsider", RoleType.Outsider, Edition.Carousel),
            new Role("car-m", "carousel-minion", RoleType.Minion, Edition.Carousel),
            new Role("car-d", "carousel-demon", RoleType.Demon, Edition.Carousel),
            new Role("car-t", "carousel-traveller", RoleType.Traveller, Edition.Carousel)
        ];
    }
}