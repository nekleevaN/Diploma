using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrustMarket.UserService.Application.Users.Commands.GoogleAuth;

namespace TrustMarket.UserService.Infrastructure.Auth;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly string _clientId;
    private readonly ILogger<GoogleTokenValidator> _logger;

    public GoogleTokenValidator(IConfiguration config, ILogger<GoogleTokenValidator> logger)
    {
        _clientId = config["Google:ClientId"]
            ?? throw new InvalidOperationException("Google:ClientId не налаштовано");
        _logger = logger;
    }

    public async Task<GooglePayload?> ValidateAsync(string idToken, CancellationToken ct = default)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _clientId },
                    IssuedAtClockTolerance = TimeSpan.FromMinutes(5)
                });

            return new GooglePayload(
                payload.Subject,
                payload.Email,
                payload.GivenName,
                payload.FamilyName);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning("Недійсний Google JWT: {Message}", ex.Message);
            return null;
        }
    }
}
