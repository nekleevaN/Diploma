using FluentValidation;
using MassTransit;
using MediatR;
using TrustMarket.ReviewService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.ReviewService.Application.Reviews.Commands;

public record SubmitReviewCommand(
    Guid ReviewId,
    Guid UserId,
    string ReviewerName,
    int Rating,
    string? Comment,
    bool IsAnonymous,
    int? DescriptionAccuracy,
    int? ShippingSpeed,
    int? Communication) : IRequest<Result>;

public class SubmitReviewCommandValidator : AbstractValidator<SubmitReviewCommand>
{
    public SubmitReviewCommandValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);

        RuleFor(x => x.Comment)
            .MaximumLength(500)
            .When(x => x.Comment is not null);

        RuleFor(x => x.Comment)
            .NotEmpty()
            .MinimumLength(20)
            .When(x => x.Rating < 3)
            .WithMessage("Коментар обов'язковий (мінімум 20 символів) для оцінки нижче 3 зірок");

        RuleFor(x => x.Comment)
            .MinimumLength(10)
            .When(x => x.Rating >= 3 && x.Comment is not null && x.Comment.Length > 0)
            .WithMessage("Коментар має бути не коротше 10 символів");

        RuleFor(x => x.DescriptionAccuracy)
            .InclusiveBetween(1, 5).When(x => x.DescriptionAccuracy.HasValue);

        RuleFor(x => x.ShippingSpeed)
            .InclusiveBetween(1, 5).When(x => x.ShippingSpeed.HasValue);

        RuleFor(x => x.Communication)
            .InclusiveBetween(1, 5).When(x => x.Communication.HasValue);
    }
}

public class SubmitReviewCommandHandler
    : IRequestHandler<SubmitReviewCommand, Result>
{
    private readonly IReviewRepository _repo;
    private readonly IPublishEndpoint _bus;

    public SubmitReviewCommandHandler(IReviewRepository repo, IPublishEndpoint bus)
    {
        _repo = repo;
        _bus = bus;
    }

    public async Task<Result> Handle(SubmitReviewCommand req, CancellationToken ct)
    {
        var review = await _repo.GetByIdAsync(req.ReviewId, ct);

        if (review is null)
            return Result.Failure("Відгук не знайдено");

        if (!review.CanBeSubmittedBy(req.UserId))
            return Result.Failure("Ви не можете залишити цей відгук");

        review.Submit(
            req.Rating, req.Comment, req.IsAnonymous,
            req.DescriptionAccuracy, req.ShippingSpeed, req.Communication,
            req.ReviewerName);

        _repo.Update(review);
        await _repo.SaveChangesAsync(ct);

        await _bus.Publish(new ReviewPublishedIntegrationEvent(
            review.Id, review.RevieweeId, review.Type.ToString(), review.Rating!.Value), ct);

        return Result.Success();
    }
}
