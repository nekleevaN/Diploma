using MediatR;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.CatalogService.Application.Advertisements.Queries;

public record GetAdvertisementsQuery(
    string? Category,
    string? Search,
    int Page = 1,
    int PageSize = 20,
    string? CategorySub = null,
    string? CategoryItem = null,
    string? Condition = null,
    string? Brand = null,
    decimal? PriceMin = null,
    decimal? PriceMax = null,
    string? Size = null,
    string? Color = null,
    string? SortBy = null) : IRequest<Result<PagedResult<AdvertisementListItemDto>>>;

public record AdvertisementListItemDto(
    Guid Id,
    string Title,
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
    List<string> ImageUrls,
    double? Latitude,
    double? Longitude,
    string? LocationAddress);

public record PagedResult<T>(List<T> Items, int Total, int Page, int PageSize);

public class GetAdvertisementsQueryHandler
    : IRequestHandler<GetAdvertisementsQuery, Result<PagedResult<AdvertisementListItemDto>>>
{
    private readonly IAdvertisementRepository _repository;

    public GetAdvertisementsQueryHandler(IAdvertisementRepository repository)
        => _repository = repository;

    public async Task<Result<PagedResult<AdvertisementListItemDto>>> Handle(
        GetAdvertisementsQuery request, CancellationToken ct)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var page = Math.Max(1, request.Page);

        var (items, total) = await _repository.GetPagedAsync(
            request.Category, request.Search, page, pageSize, ct,
            request.CategorySub, request.CategoryItem,
            request.Condition, request.Brand,
            request.PriceMin, request.PriceMax,
            request.Size, request.Color, request.SortBy);

        var dtos = items.Select(a => new AdvertisementListItemDto(
            a.Id, a.Title, a.Price, a.Category,
            a.CategorySub, a.CategoryItem, a.CategoryLabel,
            a.Condition, a.Brand, a.Size, a.Color,
            a.SellerId, a.SellerName, a.SellerRating, a.Status.ToString(), a.ImageUrls,
            a.Latitude, a.Longitude, a.LocationAddress)).ToList();

        return Result.Success(new PagedResult<AdvertisementListItemDto>(dtos, total, page, pageSize));
    }
}
