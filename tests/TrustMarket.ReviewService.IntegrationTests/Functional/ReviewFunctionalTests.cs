using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TrustMarket.ReviewService.Infrastructure.Persistence;
using TrustMarket.ReviewService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.TestInfrastructure.Helpers;
using Xunit;

namespace TrustMarket.ReviewService.IntegrationTests.Functional;

[Collection("ReviewService")]
public class ReviewFunctionalTests : IAsyncLifetime
{
    private readonly ReviewServiceWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _buyerId = Guid.NewGuid();
    private readonly Guid _sellerId = Guid.NewGuid();
    private readonly Guid _otherId = Guid.NewGuid();

    public ReviewFunctionalTests(PostgresContainerFixture postgres)
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

    private async Task<Guid> CreatePlaceholderAsync(Guid reviewerId, Guid revieweeId)
    {
        AuthAs(reviewerId);
        var resp = await _client.PostAsJsonAsync($"/api/reviews/orders/{Guid.NewGuid()}/init", new
        {
            buyerId = reviewerId,
            sellerId = revieweeId,
            buyerName = "Покупець",
            sellerName = "Продавець"
        });
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("reviewId").GetGuid();
    }

    [Fact]
    public async Task SubmitReview_ByNonOwner_Returns400()
    {
        var reviewId = await CreatePlaceholderAsync(_buyerId, _sellerId);

        AuthAs(_otherId); // different user
        var resp = await _client.PostAsJsonAsync($"/api/reviews/{reviewId}/submit", new
        {
            rating = 4,
            comment = "OK",
            isAnonymous = false
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubmitReview_AlreadyPublished_Returns400()
    {
        var reviewId = await CreatePlaceholderAsync(_buyerId, _sellerId);

        AuthAs(_buyerId);
        await _client.PostAsJsonAsync($"/api/reviews/{reviewId}/submit", new
        {
            rating = 5,
            comment = "Перший",
            isAnonymous = false
        });

        var secondResp = await _client.PostAsJsonAsync($"/api/reviews/{reviewId}/submit", new
        {
            rating = 3,
            comment = "Другий",
            isAnonymous = false
        });

        secondResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
