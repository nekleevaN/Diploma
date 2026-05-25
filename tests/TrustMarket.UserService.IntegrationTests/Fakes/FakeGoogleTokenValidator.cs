using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Application.Users.Commands.GoogleAuth;

namespace TrustMarket.UserService.IntegrationTests.Fakes;

public class FakeGoogleTokenValidator : IGoogleTokenValidator
{
    public GooglePayload? NextPayload { get; set; }

    public Task<GooglePayload?> ValidateAsync(string idToken, CancellationToken ct = default)
        => Task.FromResult(NextPayload);
}
