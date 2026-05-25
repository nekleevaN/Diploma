using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TrustMarket.TestInfrastructure.Helpers;

public static class JwtTokenHelper
{
    private const string Secret = "ThisIsASecretKeyForDevelopmentOnly_ChangeInProductionPLEASE_min32chars";
    private const string Issuer = "TrustMarket";
    private const string Audience = "TrustMarket.Users";

    public static string GenerateToken(
        Guid userId,
        bool emailConfirmed = true,
        string role = "User",
        string username = "testuser",
        string displayName = "Test User",
        double rating = 5.0)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("email_confirmed", emailConfirmed ? "true" : "false"),
            new(ClaimTypes.Role, role),
            new("username", username),
            new("display_name", displayName),
            new("rating", rating.ToString(CultureInfo.InvariantCulture)),
        };

        return BuildToken(claims, DateTime.UtcNow.AddHours(1));
    }

    public static string GenerateExpiredToken(Guid userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("email_confirmed", "true"),
        };

        return BuildToken(claims, DateTime.UtcNow.AddHours(-1));
    }

    private static string BuildToken(IEnumerable<Claim> claims, DateTime expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
