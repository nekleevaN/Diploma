using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TrustMarket.NotificationService.Application.Abstractions;

namespace TrustMarket.NotificationService.Infrastructure.Telegram;

public class TelegramNotifier : ITelegramNotifier
{
    private readonly ITelegramBotClient _botClient;
    private readonly long _adminChatId;
    private readonly ILogger<TelegramNotifier> _logger;

    public TelegramNotifier(ITelegramBotClient botClient, IConfiguration configuration, ILogger<TelegramNotifier> logger)
    {
        _botClient = botClient;
        _adminChatId = long.Parse(configuration["Telegram:AdminChatId"] ?? "0");
        _logger = logger;
    }

    public async Task SendToAdminAsync(string message, CancellationToken ct = default)
    {
        if (_adminChatId == 0)
        {
            _logger.LogWarning("Telegram:AdminChatId не налаштовано, повідомлення пропущено");
            return;
        }

        try
        {
            await _botClient.SendTextMessageAsync(_adminChatId, message, parseMode: ParseMode.Markdown, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка відправки Telegram повідомлення");
        }
    }

    public async Task SendToUserAsync(long chatId, string message, CancellationToken ct = default)
    {
        try
        {
            await _botClient.SendTextMessageAsync(chatId, message, parseMode: ParseMode.Markdown, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка відправки Telegram повідомлення користувачу {ChatId}", chatId);
        }
    }
}
