using MassTransit;
using Microsoft.Extensions.Logging;
using TrustMarket.UserService.Domain.Repositories;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.UserService.Infrastructure.Messaging;

public class ReviewPublishedConsumer : IConsumer<ReviewPublishedIntegrationEvent>
{
    private readonly IUserRepository _repo;
    private readonly ILogger<ReviewPublishedConsumer> _logger;

    public ReviewPublishedConsumer(IUserRepository repo, ILogger<ReviewPublishedConsumer> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReviewPublishedIntegrationEvent> context)
    {
        var evt = context.Message;

        var user = await _repo.GetByIdAsync(evt.RevieweeId, context.CancellationToken);
        if (user is null)
        {
            _logger.LogWarning("ReviewPublished: user {UserId} not found", evt.RevieweeId);
            return;
        }

        var asSeller = evt.ReviewType == "BuyerToSeller";
        user.UpdateReviewRating(asSeller, evt.Rating);

        _repo.Update(user);
        await _repo.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "User {UserId} {Role}Rating updated: {Rating} (review #{ReviewId})",
            evt.RevieweeId, asSeller ? "Seller" : "Buyer", evt.Rating, evt.ReviewId);
    }
}
