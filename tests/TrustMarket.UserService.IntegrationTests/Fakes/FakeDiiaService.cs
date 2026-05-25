using TrustMarket.UserService.Application.Abstractions;

namespace TrustMarket.UserService.IntegrationTests.Fakes;

public class FakeDiiaService : IDiiaService
{
    public string? NextSessionId { get; set; } = "test-session-id";
    public DiiaVerificationResult? NextResult { get; set; }

    public Task<string> StartVerificationAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult(NextSessionId ?? "session-fallback");

    public Task<DiiaVerificationResult?> VerifyAsync(string sessionId, CancellationToken ct = default)
        => Task.FromResult(NextResult);
}
