using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TrustMarket.CatalogService.Infrastructure.Persistence;
using TrustMarket.CatalogService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.TestInfrastructure.Helpers;
using Xunit;

namespace TrustMarket.CatalogService.IntegrationTests.Functional;

[Collection("CatalogService")]
public class AdvertisementFunctionalTests : IAsyncLifetime
{
    private readonly CatalogServiceWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _sellerId = Guid.NewGuid();
    private readonly Guid _buyerId = Guid.NewGuid();

    public AdvertisementFunctionalTests(PostgresContainerFixture postgres)
    {
        _factory = new CatalogServiceWebAppFactory(postgres);
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private void AuthAs(Guid userId) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", JwtTokenHelper.GenerateToken(userId));

    [Fact]
    public async Task CreateAd_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.PostAsJsonAsync("/api/ads", new
        {
            title = "Test",
            description = "Test",
            price = 100m,
            category = "Test"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAd_EmailNotConfirmed_Returns403()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", JwtTokenHelper.GenerateToken(_sellerId, emailConfirmed: false));

        var response = await _client.PostAsJsonAsync("/api/ads", new
        {
            title = "Test",
            description = "Test",
            price = 100m,
            category = "Test"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteAd_ByNonOwner_Returns400OrForbidden()
    {
        // Seller creates ad
        AuthAs(_sellerId);
        var createResp = await _client.PostAsJsonAsync("/api/ads", new
        {
            title = "Товар продавця",
            description = "Опис",
            price = 500m,
            category = "Одяг"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var adId = body.GetProperty("advertisementId").GetGuid();

        // Buyer tries to delete it
        AuthAs(_buyerId);
        var deleteResp = await _client.DeleteAsync($"/api/ads/{adId}");

        deleteResp.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ReserveAd_AlreadyReserved_Returns409()
    {
        AuthAs(_sellerId);
        var createResp = await _client.PostAsJsonAsync("/api/ads", new
        {
            title = "Телефон",
            description = "Гарний телефон",
            price = 10000m,
            category = "Електроніка"
        });
        var body = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var adId = body.GetProperty("advertisementId").GetGuid();

        AuthAs(_buyerId);
        await _client.PostAsync($"/api/ads/{adId}/reserve", null);
        var secondReserve = await _client.PostAsync($"/api/ads/{adId}/reserve", null);

        secondReserve.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
