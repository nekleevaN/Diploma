using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrustMarket.CatalogService.Application.Offers.Commands;
using TrustMarket.CatalogService.Application.Offers.Queries;

namespace TrustMarket.CatalogService.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class OffersController : ControllerBase
{
    private readonly IMediator _mediator;
    public OffersController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string CurrentUsername => User.FindFirstValue("username") ?? "Покупець";

    [HttpPost("ads/{adId:guid}/offers")]
    public async Task<IActionResult> MakeOffer(Guid adId, [FromBody] MakeOfferRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateOfferCommand(adId, CurrentUserId, CurrentUsername, request.OfferedPrice), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("ads/{adId:guid}/offers")]
    public async Task<IActionResult> GetAdOffers(Guid adId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAdOffersQuery(adId, CurrentUserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("offers/my")]
    public async Task<IActionResult> GetMyOffers(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyOffersQuery(CurrentUserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("offers/pending-count")]
    public async Task<IActionResult> GetPendingCount(CancellationToken ct)
    {
        var count = await _mediator.Send(new GetPendingOffersCountQuery(CurrentUserId), ct);
        return Ok(new { count });
    }

    [HttpPut("offers/{offerId:guid}/respond")]
    public async Task<IActionResult> Respond(Guid offerId, [FromBody] RespondToOfferRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RespondToOfferCommand(offerId, CurrentUserId, request.Action, request.CounterPrice, request.Note), ct);
        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }

    [HttpPost("offers/{offerId:guid}/accept-counter")]
    public async Task<IActionResult> AcceptCounter(Guid offerId, CancellationToken ct)
    {
        var result = await _mediator.Send(new AcceptCounterOfferCommand(offerId, CurrentUserId), ct);
        return result.IsSuccess
            ? Ok(new { agreedPrice = result.Value })
            : BadRequest(new { error = result.Error });
    }
}

[ApiController]
[Route("api/ads")]
[Authorize]
public class AdImagesController : ControllerBase
{
    private readonly Application.Abstractions.IAdvertisementRepository _adRepo;
    public AdImagesController(Application.Abstractions.IAdvertisementRepository adRepo) => _adRepo = adRepo;

    [HttpPost("{id:guid}/images")]
    public async Task<IActionResult> AddImage(Guid id, [FromBody] AddImageRequest request, CancellationToken ct)
    {
        var sellerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ad = await _adRepo.GetByIdAsync(id, ct);
        if (ad is null) return NotFound();
        if (ad.SellerId != sellerId) return Forbid();

        ad.AddImage(request.Url);
        _adRepo.Update(ad);
        await _adRepo.SaveChangesAsync(ct);
        return Ok(new { imageUrls = ad.ImageUrls });
    }
}

[ApiController]
[Route("api/ads")]
public class AdsBySellerController : ControllerBase
{
    private readonly Application.Abstractions.IAdvertisementRepository _adRepo;
    public AdsBySellerController(Application.Abstractions.IAdvertisementRepository adRepo) => _adRepo = adRepo;

    [HttpGet("by-seller/{sellerId:guid}")]
    public async Task<IActionResult> BySeller(Guid sellerId, CancellationToken ct)
    {
        var (items, total) = await _adRepo.GetPagedAsync(null, null, 1, 50, ct);
        var sellerAds = items.Where(a => a.SellerId == sellerId)
            .Select(a => new
            {
                id = a.Id, title = a.Title, price = a.Price,
                category = a.Category, status = a.Status.ToString(),
                sellerName = a.SellerName, sellerRating = a.SellerRating,
                imageUrls = a.ImageUrls
            }).ToList();
        return Ok(sellerAds);
    }
}

public record MakeOfferRequest(decimal OfferedPrice);
public record RespondToOfferRequest(string Action, decimal? CounterPrice, string? Note);
public record AddImageRequest(string Url);
