using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrustMarket.CatalogService.Application.Advertisements.Commands;
using TrustMarket.CatalogService.Application.Advertisements.Queries;
using TrustMarket.CatalogService.Domain.Entities;
using TrustMarket.CatalogService.Infrastructure.Persistence;

namespace TrustMarket.CatalogService.Api.Controllers;

[ApiController]
[Route("api/ads")]
public class AdvertisementsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly CatalogDbContext _db;

    public AdvertisementsController(IMediator mediator, CatalogDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] string? categorySub,
        [FromQuery] string? categoryItem,
        [FromQuery] string? search,
        [FromQuery] string? condition,
        [FromQuery] string? brand,
        [FromQuery] decimal? priceMin,
        [FromQuery] decimal? priceMax,
        [FromQuery] string? size,
        [FromQuery] string? color,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAdvertisementsQuery(category, search, page, pageSize,
                categorySub, categoryItem, condition, brand, priceMin, priceMax, size, color, sortBy), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAdvertisementByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpPost]
    [Authorize(Policy = "EmailConfirmed")]
    public async Task<IActionResult> Create([FromBody] CreateAdRequest request, CancellationToken ct)
    {
        var sellerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sellerName = User.FindFirstValue("display_name")
                      ?? User.FindFirstValue("username")
                      ?? "Продавець";
        var sellerRating = double.TryParse(User.FindFirstValue("rating"), out var r) ? r : 5.0;
        var sellerSubMerchantId = User.FindFirstValue("sub_merchant_id");

        var result = await _mediator.Send(new CreateAdvertisementCommand(
            request.Title, request.Description, request.Price, request.Category,
            sellerId, sellerName, sellerRating, sellerSubMerchantId,
            request.CategorySub, request.CategoryItem, request.CategoryLabel,
            request.Condition, request.Brand, request.Size, request.Color,
            request.Latitude, request.Longitude, request.LocationAddress), ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.AdvertisementId }, result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "EmailConfirmed")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAdRequest request, CancellationToken ct)
    {
        var sellerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new UpdateAdvertisementCommand(
            id, sellerId, request.Title, request.Description, request.Price, request.Category,
            request.CategorySub, request.CategoryItem, request.CategoryLabel,
            request.Condition, request.Brand, request.Size, request.Color,
            request.Latitude, request.Longitude, request.LocationAddress, request.ClearLocation), ct);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "EmailConfirmed")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var sellerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new DeleteAdvertisementCommand(id, sellerId), ct);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}/images")]
    [Authorize]
    public async Task<IActionResult> ReplaceImages(Guid id, [FromBody] ReplaceImagesRequest req, CancellationToken ct)
    {
        var ad = await _db.Advertisements.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (ad is null) return NotFound();

        ad.ReplaceImages(req.Urls);

        _db.Advertisements.Update(ad);
        await _db.SaveChangesAsync(ct);
        return Ok(new { count = ad.ImageUrls.Count });
    }

    [HttpPost("{id:guid}/reserve")]
    [Authorize]
    public async Task<IActionResult> Reserve(Guid id, CancellationToken ct)
    {
        var buyerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new ReserveAdvertisementCommand(id, buyerId), ct);

        if (!result.IsSuccess)
        {
            if (result.Error!.StartsWith("CONFLICT:"))
                return Conflict(new { error = result.Error[9..] });
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/unreserve")]
    [Authorize]
    public async Task<IActionResult> Unreserve(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new UnreserveAdvertisementCommand(id), ct);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpGet("map")]
    public async Task<IActionResult> GetForMap(CancellationToken ct)
    {
        var ads = await _db.Advertisements
            .Where(a => a.Status == AdvertisementStatus.Active &&
                        a.Latitude != null && a.Longitude != null)
            .Select(a => new
            {
                id = a.Id,
                title = a.Title,
                price = a.Price,
                category = a.Category,
                sellerId = a.SellerId,
                sellerName = a.SellerName,
                imageUrls = a.ImageUrls,
                latitude = a.Latitude,
                longitude = a.Longitude,
                locationAddress = a.LocationAddress
            })
            .ToListAsync(ct);

        return Ok(ads);
    }
}

public record ReplaceImagesRequest(List<string> Urls);

public class CreateAdRequest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public string Category { get; set; } = "";
    public string? CategorySub { get; set; }
    public string? CategoryItem { get; set; }
    public string? CategoryLabel { get; set; }
    public string? Condition { get; set; }
    public string? Brand { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? LocationAddress { get; set; }
}

public record UpdateAdRequest(
    string Title, string Description, decimal Price, string Category,
    string? CategorySub = null, string? CategoryItem = null, string? CategoryLabel = null,
    string? Condition = null, string? Brand = null, string? Size = null, string? Color = null,
    double? Latitude = null, double? Longitude = null, string? LocationAddress = null,
    bool ClearLocation = false);
