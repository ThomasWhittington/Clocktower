using Clocktower.Server.Data;
using Clocktower.Server.Data.Stores;

namespace Clocktower.ServerTests.Data.Stores;

[TestClass]
public class UserIdentityStoreTests
{
    private IUserIdentityStore _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _sut = new UserIdentityStore();
    }


    [TestMethod]
    public void UpdateIdentity_AddsIdentity_WhenCurrentlyNotExists()
    {
        var townUser = CommonMethods.GetRandomTownUser();

        _sut.UpdateIdentity(townUser);

        var record = _sut.GetIdentity(townUser.Id);
        record.Should().NotBeNull();
        record.Should().Be(townUser);
    }

    [TestMethod]
    public void UpdateIdentity_UpdatesIdentity_WhenCurrentlyExists()
    {
        const string userId = "123";
        var townUser = CommonMethods.GetRandomTownUser(userId);
        _sut.UpdateIdentity(townUser);
        var townUser2 = CommonMethods.GetRandomTownUser(userId);
        _sut.UpdateIdentity(townUser2);

        var record = _sut.GetIdentity(userId);
        record.Should().NotBeNull();
        _sut.GetAllIdentities().Should().HaveCount(1);
        record.Should().Be(townUser2);
    }

    [TestMethod]
    public void GetIdentity_ReturnsNull_WhenIdentityDoesNotExist()
    {
        const string userId = "123";

        var record = _sut.GetIdentity(userId);

        record.Should().BeNull();
    }

    [TestMethod]
    public void GetIdentity_ReturnsCorrectRecord_WhenMultipleUsersStored()
    {
        const string userId1 = "123";
        const string userId2 = "789";
        var townUser1 = CommonMethods.GetRandomTownUser(userId1);
        var townUser2 = CommonMethods.GetRandomTownUser(userId2);
        _sut.UpdateIdentity(townUser1);
        _sut.UpdateIdentity(townUser2);

        var record = _sut.GetIdentity(userId2);

        record.Should().NotBeNull();
        record.Should().Be(townUser2);
    }

    [TestMethod]
    public void GetAllIdentities_ReturnsAllRecords()
    {
        const string userId1 = "123";
        const string userId2 = "789";
        var townUser1 = CommonMethods.GetRandomTownUser(userId1);
        var townUser2 = CommonMethods.GetRandomTownUser(userId2);
        _sut.UpdateIdentity(townUser1);
        _sut.UpdateIdentity(townUser2);

        var record = _sut.GetAllIdentities();

        record.Should().HaveCount(2);
        record.Should().Contain(new KeyValuePair<string, TownUser>(userId1, townUser1));
        record.Should().Contain(new KeyValuePair<string, TownUser>(userId2, townUser2));
    }
}