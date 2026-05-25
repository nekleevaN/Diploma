using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using TrustMarket.UserService.Application.Abstractions;

namespace TrustMarket.UserService.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public Task SendEmailConfirmationAsync(
        string to, string firstName, string confirmationUrl, CancellationToken ct = default)
        => SendAsync(
            to,
            "Підтвердіть ваш email на TrustMarket",
            EmailTemplates.ConfirmEmail(firstName, confirmationUrl),
            ct);

    public Task SendPasswordResetAsync(
        string to, string firstName, string resetUrl, CancellationToken ct = default)
        => SendAsync(
            to,
            "Скидання паролю TrustMarket",
            EmailTemplates.ResetPassword(firstName, resetUrl),
            ct);

    public Task SendWelcomeAsync(
        string to, string firstName, CancellationToken ct = default)
        => SendAsync(to, $"Ласкаво просимо на TrustMarket, {firstName}!", EmailTemplates.Welcome(firstName), ct);

    public Task SendRawAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
        => SendAsync(to, subject, htmlBody, ct);

    private async Task SendAsync(
        string to, string subject, string htmlBody, CancellationToken ct)
    {
        var host     = _config["Email:SmtpHost"]    ?? throw new InvalidOperationException("Email:SmtpHost not configured");
        var port     = int.Parse(_config["Email:SmtpPort"] ?? "587");
        var username = _config["Email:Username"]    ?? throw new InvalidOperationException("Email:Username not configured");
        var password = _config["Email:Password"]    ?? throw new InvalidOperationException("Email:Password not configured");
        var from     = _config["Email:FromAddress"] ?? username;

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse($"TrustMarket <{from}>"));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body    = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(username, password, ct);
            await client.SendAsync(message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP помилка при відправці листа на {To}", to);
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true, ct);
        }
    }
}
