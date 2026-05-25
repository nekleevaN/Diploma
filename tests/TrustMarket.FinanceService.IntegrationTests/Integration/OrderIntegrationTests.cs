using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Infrastructure.Persistence;
using TrustMarket.FinanceService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.TestInfrastructure.Helpers;
using Xunit;

namespace TrustMarket.FinanceService.IntegrationTests.Integration;

[Collection("FinanceService")]
public class OrderIntegrationTests : IAsyncLifetime
{
    private readonly FinanceServiceWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _buyerId = Guid.NewGuid();
    private readonly Guid _sellerId = Guid.NewGuid();
    private readonly Guid _adId = Guid.NewGuid();

    public OrderIntegrationTests(PostgresContainerFixture postgres)
    {
        _factory = new FinanceServiceWebAppFactory(postgres);
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", JwtTokenHelper.GenerateToken(_buyerId));
    }

    public async Task InitializeAsync()
    {
        _factory.CatalogClient.NextReservationResult = new(
            true, false, null,
            new(_sellerId, "Тестовий товар", 1000m, null));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateOrder_ValidRequest_SavesOrderWithPendingStatus()
    {
        var response = await _client.PostAsJsonAsync("/api/payment/create", new
        {
            advertisementId = _adId,
            sellerId = _sellerId,
            adTitle = "Тестовий товар",
            amount = 1000m
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        var order = await db.Orders.FirstOrDefaultAsync(o => o.BuyerId == _buyerId);

        order.Should().NotBeNull();
        order!.Status.Should().Be(OrderStatus.Pending);
    }
}
