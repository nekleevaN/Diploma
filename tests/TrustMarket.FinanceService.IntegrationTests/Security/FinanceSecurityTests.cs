using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TrustMarket.FinanceService.Infrastructure.Persistence;
using TrustMarket.FinanceService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.TestInfrastructure.Helpers;
using Xunit;

namespace TrustMarket.FinanceService.IntegrationTests.Security;

[Collection("FinanceService")]
public class FinanceSecurityTests : IAsyncLifetime
{
    private readonly FinanceServiceWebAppFactory _factory;
    private readonly HttpClient _client;

    public FinanceSecurityTests(PostgresContainerFixture postgres)
    {
        _factory = new FinanceServiceWebAppFactory(postgres);
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateOrder_NoToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var resp = await _client.PostAsJsonAsync("/api/payment/create", new
        {
            advertisementId = Guid.NewGuid(),
            sellerId = Guid.NewGuid(),
            adTitle = "X",
            amount = 100m
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_ExpiredToken_Returns401()
    {
        var expired = JwtTokenHelper.GenerateExpiredToken(Guid.NewGuid());
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", expired);

        var resp = await _client.PostAsJsonAsync("/api/payment/create", new
        {
            advertisementId = Guid.NewGuid(),
            sellerId = Guid.NewGuid(),
            adTitle = "X",
            amount = 100m
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyOrders_WithoutToken_Returns401()
    {
        var resp = await _client.GetAsync("/api/payment/my/buyer");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
