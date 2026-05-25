using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TrustMarket.ReviewService.Domain.Entities;
using TrustMarket.ReviewService.Infrastructure.Persistence;
using TrustMarket.ReviewService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.TestInfrastructure.Helpers;
using Xunit;

namespace TrustMarket.ReviewService.IntegrationTests.Integration;

[Collection("ReviewService")]
public class ReviewIntegrationTests : IAsyncLifetime
{
    private readonly ReviewServiceWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _reviewerId = Guid.NewGuid();
    private readonly Guid _revieweeId = Guid.NewGuid();

    public ReviewIntegrationTests(PostgresContainerFixture postgres)
    {
        _factory = new ReviewServiceWebAppFactory(postgres);
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", JwtTokenHelper.GenerateToken(_reviewerId));
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SubmitReview_ValidRequest_SavesWithPublishedStatus()
    {
        // Create placeholder first via init endpoint
        var orderId = Guid.NewGuid();
        var initResp = await _client.PostAsJsonAsync($"/api/reviews/orders/{orderId}/init", new
        {
            buyerId = _reviewerId,
            sellerId = _revieweeId,
            buyerName = "Покупець",
            sellerName = "Продавець"
        });
        initResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var initBody = await initResp.Content.ReadFromJsonAsync<JsonElement>();
        var reviewId = initBody.GetProperty("reviewId").GetGuid();

        // Submit the review
        var submitResp = await _client.PostAsJsonAsync($"/api/reviews/{reviewId}/submit", new
        {
            rating = 5,
            comment = "Чудовий продавець!",
            isAnonymous = false,
            descriptionAccuracy = 5,
            shippingSpeed = 5,
            communication = 5
        });
        submitResp.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
        var review = await db.Reviews.FindAsync(reviewId);

        review!.Status.Should().Be(ReviewStatus.Published);
    }
}
