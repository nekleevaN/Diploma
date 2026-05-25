using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrustMarket.ReviewService.Application.Reviews.Commands;
using TrustMarket.ReviewService.Application.Reviews.Queries;

namespace TrustMarket.ReviewService.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReviewsController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string CurrentDisplayName =>
        User.FindFirstValue("display_name") ?? User.FindFirstValue("username") ?? "Користувач";


    [HttpPost("{reviewId:guid}/submit")]
    [Authorize(Policy = "EmailConfirmed")]
    public async Task<IActionResult> Submit(
        Guid reviewId, [FromBody] SubmitReviewRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitReviewCommand(
            reviewId, CurrentUserId, CurrentDisplayName,
            req.Rating, req.Comment, req.IsAnonymous,
            req.DescriptionAccuracy, req.ShippingSpeed, req.Communication), ct);

        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }


    [HttpPut("{reviewId:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(
        Guid reviewId, [FromBody] UpdateReviewRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateReviewCommand(
            reviewId, CurrentUserId,
            req.Rating, req.Comment, req.IsAnonymous,
            req.DescriptionAccuracy, req.ShippingSpeed, req.Communication), ct);

        return result.IsSuccess ? Ok() : BadRequest(new { error = result.Error });
    }


    [HttpGet("users/{userId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserReviews(
        Guid userId,
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sort = "newest",
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetUserReviewsQuery(userId, type, page, pageSize, sort), ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }


    [HttpGet("users/{userId:guid}/stats")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRatingStats(Guid userId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserRatingStatsQuery(userId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }


    [HttpGet("my/pending")]
    [Authorize]
    public async Task<IActionResult> GetMyPending(CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetMyPendingReviewsQuery(CurrentUserId), ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }


    [HttpGet("my/submitted")]
    [Authorize]
    public async Task<IActionResult> GetMySubmitted(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMySubmittedOrderIdsQuery(CurrentUserId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }


    [HttpPost("orders/{orderId:guid}/init")]
    [Authorize]
    public async Task<IActionResult> InitOrderReviews(
        Guid orderId, [FromBody] InitReviewsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new EnsureReviewPlaceholdersCommand(
            orderId, req.BuyerId, req.SellerId, CurrentUserId,
            req.BuyerName, req.SellerName), ct);

        return result.IsSuccess
            ? Ok(new { reviewId = result.Value })
            : BadRequest(new { error = result.Error });
    }
}

public record InitReviewsRequest(
    Guid BuyerId,
    Guid SellerId,
    string BuyerName,
    string SellerName);


public record SubmitReviewRequest(
    int Rating,
    string? Comment,
    bool IsAnonymous,
    int? DescriptionAccuracy,
    int? ShippingSpeed,
    int? Communication);

public record UpdateReviewRequest(
    int Rating,
    string? Comment,
    bool IsAnonymous,
    int? DescriptionAccuracy,
    int? ShippingSpeed,
    int? Communication);
