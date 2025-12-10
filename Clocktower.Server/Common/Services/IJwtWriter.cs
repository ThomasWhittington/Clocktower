namespace Clocktower.Server.Common.Services;

public interface IJwtWriter
{
    string GetJwtToken(GameUser gameUser, bool isTest = false);
    string GetJwtToken(TownUser townUser, bool isTest = false);
    string GetJwtToken(string id, string name, bool isStoryTeller, bool testBypass = false);
}