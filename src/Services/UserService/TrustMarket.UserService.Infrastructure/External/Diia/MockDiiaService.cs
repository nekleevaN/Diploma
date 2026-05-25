using TrustMarket.UserService.Application.Abstractions;

namespace TrustMarket.UserService.Infrastructure.External.Diia;

public class MockDiiaService : IDiiaService
{
    private static readonly Dictionary<string, Guid> _sessions = new();

    public Task<string> StartVerificationAsync(Guid userId, CancellationToken ct = default)
    {
        var sessionId = Guid.NewGuid().ToString();
        _sessions[sessionId] = userId;
        return Task.FromResult(sessionId);
    }

    public Task<DiiaVerificationResult?> VerifyAsync(string sessionId, CancellationToken ct = default)
    {
        if (!_sessions.ContainsKey(sessionId))
            return Task.FromResult<DiiaVerificationResult?>(null);

        _sessions.Remove(sessionId);

        var result = new DiiaVerificationResult(
            FullName: "Тестовий Користувач Іванович",
            TaxId: "1234567890",
            BirthDate: new DateTime(1995, 5, 15));

        return Task.FromResult<DiiaVerificationResult?>(result);
    }
}
