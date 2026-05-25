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

namespace TrustMarket.ReviewService.IntegrationTests.System;

[Collection("ReviewService")]
public class ReviewSystemTests : IAsyncLifetime
{
    private readonly ReviewServiceWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _buyerId = Guid.NewGuid();
    private readonly Guid _sellerId = Guid.NewGuid();

    public ReviewSystemTests(PostgresContainerFixture postgres)
    {
        _factory = new ReviewServiceWebAppFactory(postgres);
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private void AuthAs(Guid userId) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", JwtTokenHelper.GenerateToken(userId));

    [Fact]
    public async Task InitPlaceholder_Submit_VerifyPublishedInDb()
    {
        // 1. Create placeholder
        var orderId = Guid.NewGuid();
        AuthAs(_buyerId);
        var initResp = await _client.PostAsJsonAsync($"/api/reviews/orders/{orderId}/init", new
        {
            buyerId = _buyerId,
            sellerId = _sellerId,
            buyerName = "Покупець",
            sellerName = "Продавець"
        });
        initResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var reviewId = (await initResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("reviewId").GetGuid();

        // 2. Submit the review
        var submitResp = await _client.PostAsJsonAsync($"/api/reviews/{reviewId}/submit", new
        {
            rating = 5,
            comment = "Системний тест",
            isAnonymous = false
        });
        submitResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Verify in DB
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
        var review = await db.Reviews.FindAsync(reviewId);

        review!.Status.Should().Be(ReviewStatus.Published);
        review.Rating.Should().Be(5);
    }
}
