using TrustMarket.UserService.Domain.Entities;

namespace TrustMarket.UserService.Application.Abstractions;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(
        string to, string firstName, string confirmationUrl,
        CancellationToken ct = default);

    Task SendPasswordResetAsync(
        string to, string firstName, string resetUrl,
        CancellationToken ct = default);

    Task SendWelcomeAsync(
        string to, string firstName,
        CancellationToken ct = default);

    Task SendRawAsync(
        string to, string subject, string htmlBody,
        CancellationToken ct = default);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}

public interface IDiiaService
{
    Task<DiiaVerificationResult?> VerifyAsync(string sessionId, CancellationToken ct = default);
    Task<string> StartVerificationAsync(Guid userId, CancellationToken ct = default);
}

public record DiiaVerificationResult(
    string FullName,
    string TaxId,
    DateTime BirthDate);
