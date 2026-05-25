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

namespace TrustMarket.FinanceService.IntegrationTests.System;

[Collection("FinanceService")]
public class OrderSystemTests : IAsyncLifetime
{
    private readonly FinanceServiceWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _buyerId = Guid.NewGuid();
    private readonly Guid _sellerId = Guid.NewGuid();

    public OrderSystemTests(PostgresContainerFixture postgres)
    {
        _factory = new FinanceServiceWebAppFactory(postgres);
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        _factory.CatalogClient.NextReservationResult = new(
            true, false, null,
            new(_sellerId, "Велосипед", 2000m, null));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private void AuthAs(Guid userId) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", JwtTokenHelper.GenerateToken(userId));

    [Fact]
    public async Task CreateOrder_Finalize_OrderStatusIsPaid()
    {
        // 1. Buyer creates order
        AuthAs(_buyerId);
        var createResp = await _client.PostAsJsonAsync("/api/payment/create", new
        {
            advertisementId = Guid.NewGuid(),
            sellerId = _sellerId,
            adTitle = "Велосипед",
            amount = 2000m
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var createBody = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = createBody.GetProperty("orderId").GetGuid();

        // Simulate Monobank webhook: money blocked (Hold)
        using (var webhookScope = _factory.Services.CreateScope())
        {
            var webhookDb = webhookScope.ServiceProvider.GetRequiredService<FinanceDbContext>();
            var pending = await webhookDb.Orders.FindAsync(orderId);
            pending!.MarkAsPaid(DateTime.UtcNow);
            await webhookDb.SaveChangesAsync();
        }

        // 2. Seller finalizes the order
        AuthAs(_sellerId);
        var finalizeResp = await _client.PostAsync($"/api/payment/{orderId}/finalize", null);
        finalizeResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Verify order status
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        var order = await db.Orders.FindAsync(orderId);
        order!.Status.Should().Be(OrderStatus.Completed);
    }
}
