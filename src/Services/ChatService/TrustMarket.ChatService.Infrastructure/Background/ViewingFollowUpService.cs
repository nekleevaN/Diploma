using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrustMarket.ChatService.Application.Abstractions;
using TrustMarket.ChatService.Domain.Entities;
using TrustMarket.ChatService.Infrastructure.Persistence;

namespace TrustMarket.ChatService.Infrastructure.Background;

public class ViewingFollowUpService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ViewingFollowUpService> _logger;
    private const int FollowUpDelayMinutes = 2;

    public ViewingFollowUpService(IServiceScopeFactory scopeFactory, ILogger<ViewingFollowUpService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ViewingFollowUpService запущено (follow-up через {Min} хв після перегляду)", FollowUpDelayMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            try { await ProcessFollowUpsAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "Помилка ViewingFollowUpService"); }
        }
    }

    private async Task ProcessFollowUpsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        var chatRepo = scope.ServiceProvider.GetRequiredService<IChatRepository>();

        var threshold = DateTime.UtcNow.AddMinutes(-FollowUpDelayMinutes);

        var pendingFollowUps = await db.ViewingRequests
            .Where(v => v.Status == ViewingStatus.Accepted &&
                        !v.FollowUpSent &&
                        v.ProposedDateTime <= threshold)
            .ToListAsync(ct);

        foreach (var viewing in pendingFollowUps)
        {
            var followUpContent = $"{{\"type\":\"viewing_followup\",\"viewingId\":\"{viewing.Id}\"," +
                                  $"\"adTitle\":\"{viewing.AdTitle}\"," +
                                  $"\"advertisementId\":\"{viewing.AdvertisementId}\"," +
                                  $"\"sellerId\":\"{viewing.ResponderId}\"}}";

            var msg = Message.Create(viewing.ChatId, viewing.ProposerId, followUpContent, 0, null);
            await chatRepo.SaveMessageAsync(msg, ct);

            viewing.MarkFollowUpSent();
            db.ViewingRequests.Update(viewing);

            _logger.LogInformation("Follow-up надіслано для перегляду {ViewingId}", viewing.Id);
        }

        if (pendingFollowUps.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}
