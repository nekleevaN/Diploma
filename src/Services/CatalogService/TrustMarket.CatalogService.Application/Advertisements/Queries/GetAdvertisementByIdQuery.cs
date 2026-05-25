using MediatR;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.CatalogService.Application.Advertisements.Queries;

public record GetAdvertisementByIdQuery(Guid Id) : IRequest<Result<AdvertisementDetailDto>>;

public record AdvertisementDetailDto(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    string Category,
    string? CategorySub,
    string? CategoryItem,
    string? CategoryLabel,
    string? Condition,
    string? Brand,
    string? Size,
    string? Color,
    Guid SellerId,
    string SellerName,
    double SellerRating,
    string Status,
    DateTime CreatedAt,
    List<string> ImageUrls,
    double? Latitude,
    double? Longitude,
    string? LocationAddress,
    bool IsPayoutEnabled = false);

public class GetAdvertisementByIdQueryHandler
    : IRequestHandler<GetAdvertisementByIdQuery, Result<AdvertisementDetailDto>>
{
    private readonly IAdvertisementRepository _repository;

    public GetAdvertisementByIdQueryHandler(IAdvertisementRepository repository)
        => _repository = repository;

    public async Task<Result<AdvertisementDetailDto>> Handle(
        GetAdvertisementByIdQuery request, CancellationToken ct)
    {
        var ad = await _repository.GetByIdAsync(request.Id, ct);
        if (ad is null)
            return Result.Failure<AdvertisementDetailDto>("Оголошення не знайдено");

        return Result.Success(new AdvertisementDetailDto(
            ad.Id, ad.Title, ad.Description, ad.Price, ad.Category,
            ad.CategorySub, ad.CategoryItem, ad.CategoryLabel,
            ad.Condition, ad.Brand, ad.Size, ad.Color,
            ad.SellerId, ad.SellerName, ad.SellerRating, ad.Status.ToString(), ad.CreatedAt, ad.ImageUrls,
            ad.Latitude, ad.Longitude, ad.LocationAddress,
            IsPayoutEnabled: !string.IsNullOrEmpty(ad.SellerSubMerchantId)));
    }
}
