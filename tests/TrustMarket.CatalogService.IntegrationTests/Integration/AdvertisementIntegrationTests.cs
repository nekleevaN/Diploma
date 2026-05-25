using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TrustMarket.CatalogService.Infrastructure.Persistence;
using TrustMarket.CatalogService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.TestInfrastructure.Helpers;
using Xunit;

namespace TrustMarket.CatalogService.IntegrationTests.Integration;

[Collection("CatalogService")]
public class AdvertisementIntegrationTests : IAsyncLifetime
{
    private readonly CatalogServiceWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _sellerId = Guid.NewGuid();

    public AdvertisementIntegrationTests(PostgresContainerFixture postgres)
    {
        _factory = new CatalogServiceWebAppFactory(postgres);
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", JwtTokenHelper.GenerateToken(_sellerId));
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateAd_ValidRequest_SavesAdvertisementToDb()
    {
        var response = await _client.PostAsJsonAsync("/api/ads", new
        {
            title = "Тестовий ноутбук",
            description = "Відмінний ноутбук у гарному стані",
            price = 25000m,
            category = "Електроніка"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var ad = await db.Advertisements.FirstOrDefaultAsync(a => a.Title == "Тестовий ноутбук");

        ad.Should().NotBeNull();
        ad!.Price.Should().Be(25000m);
        ad.SellerId.Should().Be(_sellerId);
    }
}
