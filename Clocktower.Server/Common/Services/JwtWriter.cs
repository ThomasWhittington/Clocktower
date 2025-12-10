using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clocktower.Server.Common.Services;

public class JwtWriter(IOptions<Secrets> secretsOptions, IUserService userService) : IJwtWriter
{
    private readonly Secrets _secrets = secretsOptions.Value;

    public string GetJwtToken(GameUser gameUser, bool isTest = false)
    {
        var userName = userService.GetUserName(gameUser.Id) ?? gameUser.Id;
        var isStoryTeller = gameUser.UserType == UserType.StoryTeller;
        return GetJwtToken(gameUser.Id, userName, isStoryTeller, isTest);
    }

    public string GetJwtToken(TownUser townUser, bool isTest = false)
    {
        return GetJwtToken(townUser.Id, townUser.Name, false, isTest);
    }

    public virtual string GetJwtToken(string id, string name, bool isStoryTeller, bool testBypass = false)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id),
            new Claim(ClaimTypes.Name, name),
            new Claim("is_storyteller", isStoryTeller ? "true" : "false"),
            new Claim("test_bypass", testBypass ? "true" : "false")
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_secrets.Jwt.SigningKey)
        );
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _secrets.ServerUri,
            audience: _secrets.Jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: credentials
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }
}