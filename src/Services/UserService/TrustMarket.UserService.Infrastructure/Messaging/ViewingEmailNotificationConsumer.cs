using MassTransit;
using Microsoft.Extensions.Logging;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.UserService.Infrastructure.Messaging;

public class ViewingEmailNotificationConsumer : IConsumer<ViewingConfirmedIntegrationEvent>
{
    private readonly IEmailSender _email;
    private readonly ILogger<ViewingEmailNotificationConsumer> _logger;

    public ViewingEmailNotificationConsumer(IEmailSender email, ILogger<ViewingEmailNotificationConsumer> logger)
    {
        _email  = email;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ViewingConfirmedIntegrationEvent> context)
    {
        var evt = context.Message;
        var ct  = context.CancellationToken;
        var dt  = evt.ViewingDateTime.ToLocalTime();
        var dateStr = $"{dt:dddd, dd MMMM yyyy} о {dt:HH:mm}";

        if (!string.IsNullOrEmpty(evt.BuyerTrustedEmail))
        {
            var subject = $"🛡️ {evt.BuyerName} іде на перегляд — {dt:dd.MM HH:mm}";
            var html = BuildEmailBody(
                contactOf: evt.BuyerName,
                buyer: evt.BuyerName,
                seller: evt.SellerName,
                adTitle: evt.AdTitle,
                dateStr: dateStr,
                location: evt.LocationAddress,
                isBuyerContact: true);
            await _email.SendRawAsync(evt.BuyerTrustedEmail, subject, html, ct);
            _logger.LogInformation("Email довіреній особі покупця: {Email}", evt.BuyerTrustedEmail);
        }

        if (!string.IsNullOrEmpty(evt.SellerTrustedEmail))
        {
            var subject = $"🛡️ До {evt.SellerName} іде покупець — {dt:dd.MM HH:mm}";
            var html = BuildEmailBody(
                contactOf: evt.SellerName,
                buyer: evt.BuyerName,
                seller: evt.SellerName,
                adTitle: evt.AdTitle,
                dateStr: dateStr,
                location: evt.LocationAddress,
                isBuyerContact: false);
            await _email.SendRawAsync(evt.SellerTrustedEmail, subject, html, ct);
            _logger.LogInformation("Email довіреній особі продавця: {Email}", evt.SellerTrustedEmail);
        }
    }

    private static string BuildEmailBody(
        string contactOf,
        string buyer, string seller,
        string adTitle, string dateStr,
        string? location, bool isBuyerContact)
    {
        var headline = isBuyerContact
            ? $"<strong>{buyer}</strong> іде на перегляд до <strong>{seller}</strong>"
            : $"До <strong>{seller}</strong> іде покупець <strong>{buyer}</strong>";

        var locationHtml = !string.IsNullOrEmpty(location)
            ? $@"<tr>
                   <td style=""padding:12px 16px;border-bottom:1px solid #f0e8df"">
                     <span style=""color:#9ca3af;font-size:13px"">📍 Місце</span><br>
                     <strong style=""color:#1a1a1a"">{location}</strong>
                   </td>
                 </tr>"
            : "";

        return $"""
            <!DOCTYPE html>
            <html lang="uk">
            <head><meta charset="UTF-8"/></head>
            <body style="margin:0;padding:0;background:#f4ede4;font-family:Inter,Arial,sans-serif">
            <table width="100%" cellpadding="0" cellspacing="0">
              <tr><td align="center" style="padding:32px 16px">
                <table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.08)">
                  <tr>
                    <td style="background:#708238;padding:20px 32px">
                      <span style="font-size:22px;font-weight:700;color:#ffffff;letter-spacing:-.5px">trustee</span>
                    </td>
                  </tr>
                  <tr>
                    <td style="padding:28px 32px 16px">
                      <p style="margin:0 0 4px;font-size:13px;color:#9ca3af;text-transform:uppercase;letter-spacing:.05em">Сповіщення безпеки</p>
                      <h1 style="margin:0 0 20px;font-size:22px;color:#1a1a1a;line-height:1.3">{headline}</h1>
                    </td>
                  </tr>
                  <tr>
                    <td style="padding:0 32px 24px">
                      <table width="100%" cellpadding="0" cellspacing="0" style="border:1px solid #f0e8df;border-radius:12px;overflow:hidden">
                        <tr>
                          <td style="padding:12px 16px;border-bottom:1px solid #f0e8df">
                            <span style="color:#9ca3af;font-size:13px">👤 Покупець</span><br>
                            <strong style="color:#1a1a1a">{buyer}</strong>
                          </td>
                        </tr>
                        <tr>
                          <td style="padding:12px 16px;border-bottom:1px solid #f0e8df">
                            <span style="color:#9ca3af;font-size:13px">🏷️ Продавець</span><br>
                            <strong style="color:#1a1a1a">{seller}</strong>
                          </td>
                        </tr>
                        <tr>
                          <td style="padding:12px 16px;border-bottom:1px solid #f0e8df">
                            <span style="color:#9ca3af;font-size:13px">📦 Товар</span><br>
                            <strong style="color:#1a1a1a">{adTitle}</strong>
                          </td>
                        </tr>
                        <tr>
                          <td style="padding:12px 16px;border-bottom:1px solid #f0e8df">
                            <span style="color:#9ca3af;font-size:13px">🕐 Час</span><br>
                            <strong style="color:#708238;font-size:16px">{dateStr}</strong>
                          </td>
                        </tr>
                        {locationHtml}
                      </table>
                    </td>
                  </tr>
                  <tr>
                    <td style="padding:0 32px 28px">
                      <div style="background:#fef3c7;border-radius:10px;padding:14px 16px">
                        <p style="margin:0;font-size:13px;color:#92400e">
                          ⚠️ Якщо <strong>{contactOf}</strong> не повернеться вчасно після перегляду — зв'яжіться з ним
                          або зверніться до поліції.
                        </p>
                      </div>
                    </td>
                  </tr>
                  <tr>
                    <td style="padding:16px 32px;border-top:1px solid #f0e8df">
                      <p style="margin:0;font-size:12px;color:#9ca3af">© 2026 TrustMarket · Автоматичне сповіщення безпеки</p>
                    </td>
                  </tr>
                </table>
              </td></tr>
            </table>
            </body></html>
            """;
    }
}
