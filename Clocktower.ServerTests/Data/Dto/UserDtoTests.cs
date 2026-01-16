using Clocktower.Server.Data;
using Clocktower.Server.Data.Dto;
using Clocktower.Server.Data.Types.Enum;
using Clocktower.Server.Data.Types.Role;

namespace Clocktower.ServerTests.Data.Dto;

[TestClass]
public class UserDtoTests
{
    private readonly TownUser _townUser = CommonMethods.GetRandomTownUser() with
    {
        VoiceState = new VoiceState(true, false, false, true),
        IsPresent = true
    };

    private readonly GameUser _gameUser = CommonMethods.GetRandomGameUser() with
    {
        UserType = UserType.Player,
        IsPlaying = true,
        SeatingPosition = 5,
        HasVoteToken = true,
        IsDead = true,
        IsMarked = true,
        Role = Role.Empath(),
        DraftRole = Role.Baron()
    };

    [TestMethod]
    public void FromTownUser_MapsCorrectly_WithoutGameUser()
    {
        var result = UserDto.FromTownUser(townUser: _townUser, gameUser: null);

        result.Id.Should().Be(_townUser.Id);
        result.Name.Should().Be(_townUser.Name);
        result.AvatarUrl.Should().Be(_townUser.AvatarUrl);
        result.VoiceState.Should().Be(_townUser.VoiceState);
        result.IsPresent.Should().Be(_townUser.IsPresent);
        result.IsPlaying.Should().BeFalse();
        result.UserType.Should().Be(UserType.Unknown);
        result.SeatingPosition.Should().Be(-1);
        result.HasVoteToken.Should().BeFalse();
        result.IsDead.Should().BeFalse();
        result.IsMarked.Should().BeFalse();
        result.Role.Should().BeNull();
        result.DraftRole.Should().BeNull();
    }

    [TestMethod]
    public void FromTownUser_MapsCorrectly_WithGameUser()
    {
        var result = UserDto.FromTownUser(townUser: _townUser, gameUser: _gameUser);

        result.Id.Should().Be(_townUser.Id);
        result.Name.Should().Be(_townUser.Name);
        result.AvatarUrl.Should().Be(_townUser.AvatarUrl);
        result.VoiceState.Should().Be(_townUser.VoiceState);
        result.IsPresent.Should().Be(_townUser.IsPresent);
        result.IsPlaying.Should().Be(_gameUser.IsPlaying);
        result.UserType.Should().Be(_gameUser.UserType);
        result.SeatingPosition.Should().Be(_gameUser.SeatingPosition);
        result.HasVoteToken.Should().Be(_gameUser.HasVoteToken);
        result.IsDead.Should().Be(_gameUser.IsDead);
        result.IsMarked.Should().Be(_gameUser.IsMarked);
        result.Role.Should().Be(_gameUser.Role);
        result.DraftRole.Should().Be(_gameUser.DraftRole);
    }

    [TestMethod]
    public void FromGameUser_MapsCorrectly_WithoutTownUser()
    {
        var result = UserDto.FromGameUser(gameUser: _gameUser, townUser: null);

        result.Id.Should().Be(_gameUser.Id);
        result.Name.Should().Be(_gameUser.Id);
        result.AvatarUrl.Should().BeEmpty();
        result.VoiceState.Should().BeEquivalentTo(new VoiceState(false, false, false, false));
        result.IsPresent.Should().BeFalse();
        result.IsPlaying.Should().Be(_gameUser.IsPlaying);
        result.UserType.Should().Be(_gameUser.UserType);
        result.SeatingPosition.Should().Be(_gameUser.SeatingPosition);
        result.HasVoteToken.Should().Be(_gameUser.HasVoteToken);
        result.IsDead.Should().Be(_gameUser.IsDead);
        result.IsMarked.Should().Be(_gameUser.IsMarked);
        result.Role.Should().Be(_gameUser.Role);
        result.DraftRole.Should().Be(_gameUser.DraftRole);
    }

    [TestMethod]
    public void FromGameUser_MapsCorrectly_WithTownUser()
    {
        var result = UserDto.FromGameUser(gameUser: _gameUser, townUser: _townUser);

        result.Id.Should().Be(_gameUser.Id);
        result.Name.Should().Be(_townUser.Name);
        result.AvatarUrl.Should().Be(_townUser.AvatarUrl);
        result.VoiceState.Should().Be(_townUser.VoiceState);
        result.IsPresent.Should().Be(_townUser.IsPresent);
        result.IsPlaying.Should().Be(_gameUser.IsPlaying);
        result.UserType.Should().Be(_gameUser.UserType);
        result.SeatingPosition.Should().Be(_gameUser.SeatingPosition);
        result.HasVoteToken.Should().Be(_gameUser.HasVoteToken);
        result.IsDead.Should().Be(_gameUser.IsDead);
        result.IsMarked.Should().Be(_gameUser.IsMarked);
        result.Role.Should().Be(_gameUser.Role);
        result.DraftRole.Should().Be(_gameUser.DraftRole);
    }
}