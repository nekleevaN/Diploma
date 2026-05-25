using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrustMarket.CatalogService.Domain.Entities;
using TrustMarket.CatalogService.Infrastructure.Persistence;
using TrustMarket.CatalogService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using Xunit;

namespace TrustMarket.CatalogService.IntegrationTests.Database;

[Collection("CatalogService")]
public class CatalogDbConstraintTests : IAsyncLifetime
{
    private readonly CatalogServiceWebAppFactory _factory;

    public CatalogDbConstraintTests(PostgresContainerFixture postgres)
        => _factory = new CatalogServiceWebAppFactory(postgres);

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CascadeDelete_RemovesOffersWhenAdDeleted()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        var sellerId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();

        var ad = Advertisement.Create(
            "Товар", "Опис", 500m, "Одяг",
            sellerId, "Продавець", 5.0);
        await db.Advertisements.AddAsync(ad);
        await db.SaveChangesAsync();

        var offer = Offer.Create(ad.Id, buyerId, "Покупець", 450m);
        await db.Offers.AddAsync(offer);
        await db.SaveChangesAsync();

        db.Advertisements.Remove(ad);
        await db.SaveChangesAsync();

        var offerCount = await db.Offers.CountAsync(o => o.AdvertisementId == ad.Id);
        offerCount.Should().Be(0);
    }

    [Fact]
    public async Task Offer_FkToAdvertisement_IsEnforced()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        var nonExistentAdId = Guid.NewGuid();
        var offer = Offer.Create(nonExistentAdId, Guid.NewGuid(), "Покупець", 100m);
        await db.Offers.AddAsync(offer);

        var act = () => db.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task MigrateAsync_CreatesAdvertisementsTable()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        var count = await db.Advertisements.CountAsync();
        count.Should().Be(0);
    }
}
