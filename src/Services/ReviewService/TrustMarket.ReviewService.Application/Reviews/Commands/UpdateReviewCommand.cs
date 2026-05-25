using FluentValidation;
using MediatR;
using TrustMarket.ReviewService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.ReviewService.Application.Reviews.Commands;

public record UpdateReviewCommand(
    Guid ReviewId,
    Guid UserId,
    int Rating,
    string? Comment,
    bool IsAnonymous,
    int? DescriptionAccuracy,
    int? ShippingSpeed,
    int? Communication) : IRequest<Result>;

public class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);

        RuleFor(x => x.Comment)
            .MaximumLength(500)
            .When(x => x.Comment is not null);

        RuleFor(x => x.Comment)
            .NotEmpty().MinimumLength(20)
            .When(x => x.Rating < 3)
            .WithMessage("Коментар обов'язковий (мінімум 20 символів) для оцінки нижче 3 зірок");

        RuleFor(x => x.DescriptionAccuracy)
            .InclusiveBetween(1, 5).When(x => x.DescriptionAccuracy.HasValue);

        RuleFor(x => x.ShippingSpeed)
            .InclusiveBetween(1, 5).When(x => x.ShippingSpeed.HasValue);

        RuleFor(x => x.Communication)
            .InclusiveBetween(1, 5).When(x => x.Communication.HasValue);
    }
}

public class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, Result>
{
    private readonly IReviewRepository _repo;

    public UpdateReviewCommandHandler(IReviewRepository repo) => _repo = repo;

    public async Task<Result> Handle(UpdateReviewCommand req, CancellationToken ct)
    {
        var review = await _repo.GetByIdAsync(req.ReviewId, ct);

        if (review is null)
            return Result.Failure("Відгук не знайдено");

        if (!review.CanBeEditedBy(req.UserId))
            return Result.Failure("Вікно редагування 24 години вже минуло");

        review.Update(
            req.Rating, req.Comment, req.IsAnonymous,
            req.DescriptionAccuracy, req.ShippingSpeed, req.Communication);

        _repo.Update(review);
        await _repo.SaveChangesAsync(ct);

        return Result.Success();
    }
}
