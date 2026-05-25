using TrustMarket.UserService.Application.Abstractions;

namespace TrustMarket.UserService.IntegrationTests.Fakes;

public class FakeEmailSender : IEmailSender
{
    public record SentEmail(string To, string Subject, string Url);

    public List<SentEmail> ConfirmationEmails { get; } = [];
    public List<SentEmail> WelcomeEmails { get; } = [];
    public List<string> PasswordResetUrls { get; } = [];

    public Task SendEmailConfirmationAsync(string to, string firstName, string confirmationUrl, CancellationToken ct = default)
    {
        ConfirmationEmails.Add(new SentEmail(to, firstName, confirmationUrl));
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string to, string firstName, string resetUrl, CancellationToken ct = default)
    {
        PasswordResetUrls.Add(resetUrl);
        return Task.CompletedTask;
    }

    public Task SendWelcomeAsync(string to, string firstName, CancellationToken ct = default)
    {
        WelcomeEmails.Add(new SentEmail(to, firstName, ""));
        return Task.CompletedTask;
    }

    public Task SendRawAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
        => Task.CompletedTask;
}
