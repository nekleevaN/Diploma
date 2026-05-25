using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Infrastructure.Persistence;
using TrustMarket.FinanceService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.TestInfrastructure.Helpers;
using Xunit;

namespace TrustMarket.FinanceService.IntegrationTests.Functional;

[Collection("FinanceService")]
public class OrderFunctionalTests : IAsyncLifetime
{
    private readonly FinanceServiceWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _buyerId = Guid.NewGuid();
    private readonly Guid _sellerId = Guid.NewGuid();

    public OrderFunctionalTests(PostgresContainerFixture postgres)
    {
        _factory = new FinanceServiceWebAppFactory(postgres);
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        _factory.CatalogClient.NextReservationResult = new(
            true, false, null,
            new(_sellerId, "Тест", 500m, null));

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
    public async Task CreateOrder_CatalogConflict_Returns409()
    {
        _factory.CatalogClient.NextReservationResult = new(
            false, true, "Товар вже зарезервований", null);

        AuthAs(_buyerId);
        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            advertisementId = Guid.NewGuid(),
            amount = 500m,
            recipientCityRef = "test",
            recipientCityName = "Київ",
            recipientWarehouseRef = "test",
            recipientWarehouseAddress = "test",
            recipientFirstName = "Тест",
            recipientLastName = "Тестовий",
            recipientPhone = "+380000000000"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetOrder_AnotherUsersOrder_Returns400OrForbidden()
    {
        AuthAs(_buyerId);
        var createResp = await _client.PostAsJsonAsync("/api/payment/create", new
        {
            advertisementId = Guid.NewGuid(),
            sellerId = _sellerId,
            adTitle = "Товар",
            amount = 500m
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = body.GetProperty("orderId").GetGuid();

        AuthAs(Guid.NewGuid()); // different user
        var getResp = await _client.GetAsync($"/api/payment/{orderId}");
        getResp.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }
}
