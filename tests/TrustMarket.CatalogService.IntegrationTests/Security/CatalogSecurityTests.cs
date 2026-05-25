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

namespace TrustMarket.CatalogService.IntegrationTests.Security;

[Collection("CatalogService")]
public class CatalogSecurityTests : IAsyncLifetime
{
    private readonly CatalogServiceWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _sellerId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    public CatalogSecurityTests(PostgresContainerFixture postgres)
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

    private void AuthAs(Guid userId, bool emailConfirmed = true) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", JwtTokenHelper.GenerateToken(userId, emailConfirmed: emailConfirmed));

    [Fact]
    public async Task CreateAd_NoToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var resp = await _client.PostAsJsonAsync("/api/ads", new
        {
            title = "X",
            description = "Y",
            price = 100m,
            category = "Z"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteAd_ByNonOwner_Returns400()
    {
        AuthAs(_sellerId);
        var createResp = await _client.PostAsJsonAsync("/api/ads", new
        {
            title = "Чужий товар",
            description = "Опис",
            price = 200m,
            category = "Різне"
        });
        var body = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var adId = body.GetProperty("advertisementId").GetGuid();

        AuthAs(_otherUserId);
        var deleteResp = await _client.DeleteAsync($"/api/ads/{adId}");
        deleteResp.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateAd_XssInTitle_StoredSafelyAndReturned()
    {
        AuthAs(_sellerId);
        var xssTitle = "<script>alert('xss')</script>Ноутбук";

        var createResp = await _client.PostAsJsonAsync("/api/ads", new
        {
            title = xssTitle,
            description = "Опис",
            price = 100m,
            category = "Електроніка"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var adId = body.GetProperty("advertisementId").GetGuid();

        var getResp = await _client.GetAsync($"/api/ads/{adId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        // JSON response doesn't execute scripts — stored as plain text is acceptable
        var getBody = await getResp.Content.ReadAsStringAsync();
        getBody.Should().Contain("script"); // stored as-is (HTML encoding is frontend responsibility)
    }
}
