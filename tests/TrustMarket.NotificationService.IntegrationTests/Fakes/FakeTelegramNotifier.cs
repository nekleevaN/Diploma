using TrustMarket.NotificationService.Application.Abstractions;

namespace TrustMarket.NotificationService.IntegrationTests.Fakes;

public class FakeTelegramNotifier : ITelegramNotifier
{
    public record SentMessage(string Message, bool IsAdmin, long? ChatId);

    public List<SentMessage> SentMessages { get; } = [];

    public Task SendToAdminAsync(string message, CancellationToken ct = default)
    {
        SentMessages.Add(new SentMessage(message, IsAdmin: true, ChatId: null));
        return Task.CompletedTask;
    }

    public Task SendToUserAsync(long chatId, string message, CancellationToken ct = default)
    {
        SentMessages.Add(new SentMessage(message, IsAdmin: false, ChatId: chatId));
        return Task.CompletedTask;
    }
}
