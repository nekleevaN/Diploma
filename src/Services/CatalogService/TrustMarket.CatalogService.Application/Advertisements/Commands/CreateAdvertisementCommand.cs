using FluentValidation;
using MassTransit;
using MediatR;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Domain.Entities;
using TrustMarket.Shared.Common.Results;
using TrustMarket.Shared.Contracts.IntegrationEvents;

namespace TrustMarket.CatalogService.Application.Advertisements.Commands;

public record CreateAdvertisementCommand(
    string Title,
    string Description,
    decimal Price,
    string Category,
    Guid SellerId,
    string SellerName,
    double SellerRating,
    string? SellerSubMerchantId = null,
    string? CategorySub = null,
    string? CategoryItem = null,
    string? CategoryLabel = null,
    string? Condition = null,
    string? Brand = null,
    string? Size = null,
    string? Color = null,
    double? Latitude = null,
    double? Longitude = null,
    string? LocationAddress = null) : IRequest<Result<CreateAdvertisementResponse>>;

public record CreateAdvertisementResponse(Guid AdvertisementId);

public class CreateAdvertisementCommandValidator : AbstractValidator<CreateAdvertisementCommand>
{
    public CreateAdvertisementCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
    }
}

public class CreateAdvertisementCommandHandler
    : IRequestHandler<CreateAdvertisementCommand, Result<CreateAdvertisementResponse>>
{
    private readonly IAdvertisementRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateAdvertisementCommandHandler(
        IAdvertisementRepository repository,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<CreateAdvertisementResponse>> Handle(
        CreateAdvertisementCommand request, CancellationToken ct)
    {
        var ad = Advertisement.Create(
            request.Title, request.Description, request.Price,
            request.Category, request.SellerId, request.SellerName, request.SellerRating,
            request.CategorySub, request.CategoryItem, request.CategoryLabel,
            request.Condition, request.Brand, request.Size, request.Color);

        if (request.Latitude.HasValue && request.Longitude.HasValue)
            ad.SetLocation(request.Latitude, request.Longitude, request.LocationAddress);

        var subMerchantId = request.SellerSubMerchantId
            ?? await _repository.GetSellerSubMerchantIdAsync(request.SellerId, ct);
        if (subMerchantId is not null)
            ad.UpdateSellerSubMerchantId(subMerchantId);

        await _repository.AddAsync(ad, ct);
        await _repository.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(
            new AdvertisementCreatedIntegrationEvent(ad.Id, ad.SellerId, ad.Title, ad.Price), ct);

        return Result.Success(new CreateAdvertisementResponse(ad.Id));
    }
}
