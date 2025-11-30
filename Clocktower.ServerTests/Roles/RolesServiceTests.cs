using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Data.Types.Role;
using Clocktower.Server.Roles.Services;

namespace Clocktower.ServerTests.Roles;

[TestClass]
public class RolesServiceTests
{
    private static IRolesService Sut => new RolesService();
    private readonly IEnumerable<Role> _dummyRoles = TestDataProvider.GetDummyRoles();


    [TestMethod]
    public void GetRoles_ReturnsDefaultAll_WhenNoFiltersAndNoListGiven()
    {
        Edition? filterEdition = null;
        RoleType? filterRoleType = null;

        var result = Sut.GetRoles(filterEdition, filterRoleType);

        result.Should().BeEquivalentTo(Role.AllRoles);
    }

    [TestMethod]
    public void GetRoles_ReturnsAll_WhenNoFilters()
    {
        Edition? filterEdition = null;
        RoleType? filterRoleType = null;

        var result = Sut.GetRoles(filterEdition, filterRoleType, _dummyRoles);

        result.Should().BeEquivalentTo(_dummyRoles);
    }

    [TestMethod]
    [DataRow(Edition.TroubleBrewing)]
    [DataRow(Edition.SectsAndViolets)]
    [DataRow(Edition.BadMoonRising)]
    [DataRow(Edition.Experimental)]
    public void GetRoles_ReturnsCorrect_WhenEditionFiltered(Edition filterEdition)
    {
        RoleType? filterRoleType = null;

        var expected = _dummyRoles.Where(o => o.Edition == filterEdition);

        var result = Sut.GetRoles(filterEdition, filterRoleType, _dummyRoles);

        result.Should().BeEquivalentTo(expected);
    }

    [TestMethod]
    [DynamicData(nameof(EditionRoleTypeTestData))]
    public void GetRoles_ReturnsCorrect_WhenRoleTypeAndEditionFiltered(Edition filterEdition, RoleType filterRoleType)
    {
        var expected = _dummyRoles.Where(o => o.Edition == filterEdition && o.Type == filterRoleType);

        var result = Sut.GetRoles(filterEdition, filterRoleType, _dummyRoles);

        result.Should().BeEquivalentTo(expected);
    }

    private static IEnumerable<object[]> EditionRoleTypeTestData()
    {
        return from
                edition in Enum.GetValues<Edition>()
            from roleType in Enum.GetValues<RoleType>()
            select new object[] { edition, roleType };
    }
}