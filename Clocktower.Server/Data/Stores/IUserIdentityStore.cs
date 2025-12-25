namespace Clocktower.Server.Data.Stores;

public interface IUserIdentityStore
{
    void UpdateIdentity(TownUser user);
    TownUser? GetIdentity(string userId);
    IDictionary<string, TownUser> GetAllIdentities();
}