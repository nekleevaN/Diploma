using MassTransit;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrustMarket.ReviewService.Application.Reviews.Commands;
using TrustMarket.ReviewService.Domain.Repositories;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.ReviewService.Infrastructure.Background;

public class ReviewMaintenanceService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReviewMaintenanceService> _logger;

    public ReviewMaintenanceService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReviewMaintenanceService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReviewMaintenanceService запущено");

        using var timer = new PeriodicTimer(TimeSpan.FromHours(6));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunExpireJobAsync(stoppingToken);
            await RunReminderJobAsync(stoppingToken);
        }
    }

    private async Task RunExpireJobAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var result = await mediator.Send(new ExpireOldReviewsCommand(), ct);

            if (result.IsSuccess && result.Value > 0)
                _logger.LogInformation("Expire job: {Count} відгуків перейшли в Expired", result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка в Expire job");
        }
    }

    private async Task RunReminderJobAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IReviewRepository>();
            var bus = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var reminderThreshold = DateTime.UtcNow.AddDays(-3);
            var candidates = await repo.GetPendingOlderThanAsync(reminderThreshold, ct);

            foreach (var review in candidates)
            {
                await bus.Publish(new ReviewReminderIntegrationEvent(
                    review.Id, review.ReviewerId, review.RevieweeId,
                    review.Type.ToString(), "Ваше замовлення"), ct);
            }

            if (candidates.Count > 0)
                _logger.LogInformation("Reminder job: надіслано {Count} нагадувань", candidates.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка в Reminder job");
        }
    }
}
