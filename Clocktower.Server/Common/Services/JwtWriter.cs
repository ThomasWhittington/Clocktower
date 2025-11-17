using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Clocktower.Server.Common.Services;

public interface IJwtWriter
{
    string GetJwtToken(string userId, string userName);
}

public class JwtWriter(IOptions<Secrets> secretsOptions) : IJwtWriter
{
    private readonly Secrets _secrets = secretsOptions.Value;

    public string GetJwtToken(string userId, string userName)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userName)
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