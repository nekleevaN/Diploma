using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TrustMarket.ReviewService.Infrastructure.Persistence;
using TrustMarket.ReviewService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.TestInfrastructure.Helpers;
using Xunit;

namespace TrustMarket.ReviewService.IntegrationTests.Security;

[Collection("ReviewService")]
public class ReviewSecurityTests : IAsyncLifetime
{
    private readonly ReviewServiceWebAppFactory _factory;
    private readonly HttpClient _client;

    public ReviewSecurityTests(PostgresContainerFixture postgres)
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

    [Fact]
    public async Task SubmitReview_WithoutToken_Returns401()
    {
        var resp = await _client.PostAsJsonAsync($"/api/reviews/{Guid.NewGuid()}/submit", new
        {
            rating = 5,
            comment = "Test",
            isAnonymous = false
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitReview_ExpiredToken_Returns401()
    {
        var expired = JwtTokenHelper.GenerateExpiredToken(Guid.NewGuid());
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", expired);

        var resp = await _client.PostAsJsonAsync($"/api/reviews/{Guid.NewGuid()}/submit", new
        {
            rating = 5,
            comment = "Test",
            isAnonymous = false
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitReview_EmailNotConfirmed_Returns403()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", JwtTokenHelper.GenerateToken(Guid.NewGuid(), emailConfirmed: false));

        var resp = await _client.PostAsJsonAsync($"/api/reviews/{Guid.NewGuid()}/submit", new
        {
            rating = 5,
            comment = "Test",
            isAnonymous = false
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
