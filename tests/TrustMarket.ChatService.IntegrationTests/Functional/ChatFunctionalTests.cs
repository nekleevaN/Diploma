using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TrustMarket.ChatService.Infrastructure.Persistence;
using TrustMarket.ChatService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.TestInfrastructure.Helpers;
using Xunit;

namespace TrustMarket.ChatService.IntegrationTests.Functional;

[Collection("ChatService")]
public class ChatFunctionalTests : IAsyncLifetime
{
    private readonly ChatServiceWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _buyerId = Guid.NewGuid();
    private readonly Guid _sellerId = Guid.NewGuid();
    private readonly Guid _outsiderId = Guid.NewGuid();

    public ChatFunctionalTests(PostgresContainerFixture postgres)
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

    private void AuthAs(Guid userId) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", JwtTokenHelper.GenerateToken(userId));

    [Fact]
    public async Task SendMessage_ToForeignChat_Returns400OrForbidden()
    {
        // Buyer starts a chat
        AuthAs(_buyerId);
        var startResp = await _client.PostAsJsonAsync("/api/chats", new
        {
            sellerId = _sellerId,
            advertisementId = Guid.NewGuid(),
            adTitle = "Товар"
        });
        var chatBody = await startResp.Content.ReadFromJsonAsync<JsonElement>();
        var chatId = chatBody.GetProperty("chatId").GetGuid();

        // Outsider tries to send a message
        AuthAs(_outsiderId);
        var msgResp = await _client.PostAsJsonAsync($"/api/chats/{chatId}/messages", new
        {
            content = "Хочу вкрасти чат"
        });

        msgResp.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task StartChat_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var resp = await _client.PostAsJsonAsync("/api/chats", new
        {
            sellerId = Guid.NewGuid(),
            advertisementId = Guid.NewGuid(),
            adTitle = "Товар"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
