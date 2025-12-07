using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clocktower.Server.Common.Services;

public interface IJwtWriter
{
    string GetJwtToken(GameUser gameUser, bool isTest = false);
}

public class JwtWriter(IOptions<Secrets> secretsOptions) : IJwtWriter
{
    private readonly Secrets _secrets = secretsOptions.Value;

    public string GetJwtToken(GameUser gameUser, bool isTest = false)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, gameUser.Id),
            new Claim(ClaimTypes.Name, gameUser.Name),
            new Claim("is_storyteller", gameUser.UserType == UserType.StoryTeller ? "true" : "false"),
            new Claim("test_bypass", isTest ? "true" : "false")
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