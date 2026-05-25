using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TrustMarket.ChatService.Infrastructure.Persistence;
using TrustMarket.ChatService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.TestInfrastructure.Helpers;
using Xunit;

namespace TrustMarket.ChatService.IntegrationTests.Security;

[Collection("ChatService")]
public class ChatSecurityTests : IAsyncLifetime
{
    private readonly ChatServiceWebAppFactory _factory;
    private readonly HttpClient _client;

    public ChatSecurityTests(PostgresContainerFixture postgres)
    {
        _factory = new ChatServiceWebAppFactory(postgres);
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetChats_WithoutToken_Returns401()
    {
        var resp = await _client.GetAsync("/api/chats");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetChat_WithoutToken_Returns401()
    {
        var resp = await _client.GetAsync($"/api/chats/{Guid.NewGuid()}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SendMessage_ExpiredToken_Returns401()
    {
        var expired = JwtTokenHelper.GenerateExpiredToken(Guid.NewGuid());
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", expired);

        var resp = await _client.PostAsJsonAsync($"/api/chats/{Guid.NewGuid()}/messages",
            new { content = "Test" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
