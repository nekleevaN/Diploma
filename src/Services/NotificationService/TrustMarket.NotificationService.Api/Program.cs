using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TrustMarket.NotificationService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "NotificationService" }));

var botClient = app.Services.GetRequiredService<ITelegramBotClient>();
var cts = new CancellationTokenSource();

botClient.StartReceiving(
    updateHandler: async (bot, update, token) =>
    {
        if (update.Message is not { } message) return;
        if (message.Text is not { } text) return;

        var chatId = message.Chat.Id;
        var firstName = message.From?.FirstName ?? "Друже";

        if (text.StartsWith("/start") || text.StartsWith("/myid"))
        {
            await bot.SendTextMessageAsync(
                chatId,
                $"👋 Привіт, {firstName}!\n\n" +
                $"Твій Telegram Chat ID:\n" +
                $"<code>{chatId}</code>\n\n" +
                $"📋 Скопіюй це число і передай людині, яка хоче вказати тебе як довірену особу в TrustMarket.\n\n" +
                $"Коли вони вкажуть тебе в профілі — ти автоматично отримуватимеш сповіщення перед їхніми переглядами.",
                parseMode: ParseMode.Html,
                cancellationToken: token);
        }
    },
    pollingErrorHandler: (bot, ex, token) =>
    {
        Log.Error(ex, "Telegram polling error");
    },
    receiverOptions: new ReceiverOptions { AllowedUpdates = [UpdateType.Message] },
    cancellationToken: cts.Token
);

Log.Information("Telegram бот запущено (@Trust_Market_Test_Bot). Команди: /start, /myid");

app.Lifetime.ApplicationStopping.Register(() => cts.Cancel());

app.Run();

public partial class Program { }
