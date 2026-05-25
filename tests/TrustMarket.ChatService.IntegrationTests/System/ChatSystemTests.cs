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

namespace TrustMarket.ChatService.IntegrationTests.System;

[Collection("ChatService")]
public class ChatSystemTests : IAsyncLifetime
{
    private readonly ChatServiceWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _buyerId = Guid.NewGuid();
    private readonly Guid _sellerId = Guid.NewGuid();

    public ChatSystemTests(PostgresContainerFixture postgres)
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
    public async Task StartChat_SendMessages_GetChat_FullFlow()
    {
        var adId = Guid.NewGuid();

        // 1. Start chat
        AuthAs(_buyerId);
        var startResp = await _client.PostAsJsonAsync("/api/chats", new
        {
            sellerId = _sellerId,
            advertisementId = adId,
            adTitle = "Ноутбук"
        });
        startResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var chatId = (await startResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("chatId").GetGuid();

        // 2. Send message as buyer
        AuthAs(_buyerId);
        await _client.PostAsJsonAsync($"/api/chats/{chatId}/messages",
            new { content = "Привіт, ще актуально?" });

        // 3. Seller replies
        AuthAs(_sellerId);
        await _client.PostAsJsonAsync($"/api/chats/{chatId}/messages",
            new { content = "Так, актуально!" });

        // 4. Get chat and verify messages
        var getResp = await _client.GetAsync($"/api/chats/{chatId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        var msgCount = await db.Messages.CountAsync(m => m.ChatId == chatId);
        msgCount.Should().Be(2);
    }
}
