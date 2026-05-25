using Microsoft.Extensions.Logging;
using TrustMarket.UserService.Application.Abstractions;

namespace TrustMarket.UserService.Infrastructure.Email;

public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) => _logger = logger;

    public Task SendEmailConfirmationAsync(
        string to, string firstName, string confirmationUrl, CancellationToken ct = default)
    {
        _logger.LogInformation("""

            ╔══════════════════════════════════════════════════════╗
            ║           EMAIL: Підтвердження пошти                 ║
            ╠══════════════════════════════════════════════════════╣
            ║  To:      {To}
            ║  Кому:    {FirstName}
            ║  Тема:    Підтвердіть ваш email на TrustMarket
            ╠══════════════════════════════════════════════════════╣
            ║  >>> CONFIRMATION URL (скопіюй у браузер):
            ║  {Url}
            ╚══════════════════════════════════════════════════════╝
            """,
            to, firstName, confirmationUrl);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(
        string to, string firstName, string resetUrl, CancellationToken ct = default)
    {
        _logger.LogInformation("""

            ╔══════════════════════════════════════════════════════╗
            ║           EMAIL: Скидання паролю                     ║
            ╠══════════════════════════════════════════════════════╣
            ║  To:      {To}
            ║  Кому:    {FirstName}
            ║  Тема:    Скидання паролю TrustMarket
            ╠══════════════════════════════════════════════════════╣
            ║  >>> RESET URL (скопіюй у браузер):
            ║  {Url}
            ╚══════════════════════════════════════════════════════╝
            """,
            to, firstName, resetUrl);

        return Task.CompletedTask;
    }

    public Task SendWelcomeAsync(
        string to, string firstName, CancellationToken ct = default)
    {
        _logger.LogInformation("[EMAIL] Welcome! To={To}, FirstName={FirstName}", to, firstName);
        return Task.CompletedTask;
    }

    public Task SendRawAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        _logger.LogInformation("[EMAIL] {Subject} → {To}", subject, to);
        return Task.CompletedTask;
    }
}
