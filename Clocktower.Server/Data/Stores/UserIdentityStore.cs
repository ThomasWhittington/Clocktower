using System.Collections.Concurrent;

namespace Clocktower.Server.Data.Stores;

public class UserIdentityStore : IUserIdentityStore
{
    private readonly ConcurrentDictionary<string, TownUser> _identities = new();

    public void UpdateIdentity(TownUser user) => _identities[user.Id] = user;

    public TownUser? GetIdentity(string userId) =>
        _identities.TryGetValue(userId, out var user) ? user : null;

    public IDictionary<string, TownUser> GetAllIdentities() => _identities;
}