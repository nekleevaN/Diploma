using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Domain.Entities;

namespace TrustMarket.UserService.Infrastructure.Auth;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration) => _configuration = configuration;

    public string GenerateToken(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var secret = jwtSection["Secret"] ?? throw new InvalidOperationException("JWT Secret не налаштовано");
        var issuer = jwtSection["Issuer"] ?? "TrustMarket";
        var audience = jwtSection["Audience"] ?? "TrustMarket.Users";
        var expiryMinutes = int.Parse(jwtSection["ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("username",        user.Username),
            new("first_name",      user.FirstName),
            new("last_name",       user.LastName),
            new("display_name",    user.DisplayName),
            new("email_confirmed", user.IsEmailConfirmed.ToString().ToLower()),
            new("auth_provider",   user.AuthProvider.ToString().ToLower()),
            new("rating",          user.Rating.ToString("F1"))
        };

        if (!string.IsNullOrEmpty(user.MonobankSubMerchantId))
            claims.Add(new Claim("sub_merchant_id", user.MonobankSubMerchantId));

        if (user.TrustedContactTelegramId.HasValue)
            claims.Add(new Claim("trusted_telegram_id", user.TrustedContactTelegramId.Value.ToString()));

        if (!string.IsNullOrEmpty(user.TrustedContactEmail))
            claims.Add(new Claim("trusted_email", user.TrustedContactEmail));

        foreach (var badge in user.Badges)
            claims.Add(new Claim("badge", badge.Type.ToString()));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
