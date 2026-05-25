using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TrustMarket.CatalogService.Domain.Entities;
using TrustMarket.CatalogService.Infrastructure.Persistence;
using TrustMarket.CatalogService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.TestInfrastructure.Helpers;
using Xunit;

namespace TrustMarket.CatalogService.IntegrationTests.System;

[Collection("CatalogService")]
public class AdOfferSystemTests : IAsyncLifetime
{
    private readonly CatalogServiceWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _sellerId = Guid.NewGuid();
    private readonly Guid _buyerId = Guid.NewGuid();

    public AdOfferSystemTests(PostgresContainerFixture postgres)
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
    public async Task CreateAd_CreateOffer_AcceptOffer_AdStatusIsReserved()
    {
        // 1. Seller creates ad
        AuthAs(_sellerId);
        var createResp = await _client.PostAsJsonAsync("/api/ads", new
        {
            title = "Велосипед",
            description = "Гарний стан",
            price = 3000m,
            category = "Спорт"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var adBody = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var adId = adBody.GetProperty("advertisementId").GetGuid();

        // 2. Buyer creates offer
        AuthAs(_buyerId);
        var offerResp = await _client.PostAsJsonAsync($"/api/ads/{adId}/offers", new
        {
            offeredPrice = 2800m
        });
        offerResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var offerBody = await offerResp.Content.ReadFromJsonAsync<JsonElement>();
        var offerId = offerBody.GetProperty("offerId").GetGuid();

        // 3. Seller accepts offer
        AuthAs(_sellerId);
        var acceptResp = await _client.PutAsJsonAsync($"/api/offers/{offerId}/respond", new
        {
            action = "accept"
        });
        acceptResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Verify ad status
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var ad = await db.Advertisements.FindAsync(adId);
        ad!.Status.Should().Be(AdvertisementStatus.Reserved);
    }
}
