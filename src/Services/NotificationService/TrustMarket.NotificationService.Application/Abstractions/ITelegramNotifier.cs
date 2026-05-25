namespace TrustMarket.NotificationService.Application.Abstractions;

public interface ITelegramNotifier
{
    Task SendToAdminAsync(string message, CancellationToken ct = default);
    Task SendToUserAsync(long chatId, string message, CancellationToken ct = default);
}
