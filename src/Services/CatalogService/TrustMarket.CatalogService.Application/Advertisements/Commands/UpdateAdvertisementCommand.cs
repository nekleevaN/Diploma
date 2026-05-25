using FluentValidation;
using MediatR;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.CatalogService.Application.Advertisements.Commands;

public record UpdateAdvertisementCommand(
    Guid AdvertisementId,
    Guid SellerId,
    string Title,
    string Description,
    decimal Price,
    string Category,
    string? CategorySub = null,
    string? CategoryItem = null,
    string? CategoryLabel = null,
    string? Condition = null,
    string? Brand = null,
    string? Size = null,
    string? Color = null,
    double? Latitude = null,
    double? Longitude = null,
    string? LocationAddress = null,
    bool ClearLocation = false) : IRequest<Result>;

public class UpdateAdvertisementCommandValidator : AbstractValidator<UpdateAdvertisementCommand>
{
    public UpdateAdvertisementCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
    }
}

public class UpdateAdvertisementCommandHandler : IRequestHandler<UpdateAdvertisementCommand, Result>
{
    private readonly IAdvertisementRepository _repository;

    public UpdateAdvertisementCommandHandler(IAdvertisementRepository repository)
        => _repository = repository;

    public async Task<Result> Handle(UpdateAdvertisementCommand request, CancellationToken ct)
    {
        var ad = await _repository.GetByIdAsync(request.AdvertisementId, ct);
        if (ad is null)
            return Result.Failure("Оголошення не знайдено");

        if (ad.SellerId != request.SellerId)
            return Result.Failure("Доступ заборонено");

        ad.UpdateDetails(request.Title, request.Description, request.Price, request.Category,
            request.CategorySub, request.CategoryItem, request.CategoryLabel,
            request.Condition, request.Brand, request.Size, request.Color);

        if (request.ClearLocation)
            ad.SetLocation(null, null, null);
        else if (request.Latitude.HasValue && request.Longitude.HasValue)
            ad.SetLocation(request.Latitude, request.Longitude, request.LocationAddress);

        _repository.Update(ad);
        await _repository.SaveChangesAsync(ct);

        return Result.Success();
    }
}
