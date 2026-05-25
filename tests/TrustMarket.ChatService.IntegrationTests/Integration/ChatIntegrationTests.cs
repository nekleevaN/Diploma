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

namespace TrustMarket.ChatService.IntegrationTests.Integration;

[Collection("ChatService")]
public class ChatIntegrationTests : IAsyncLifetime
{
    private readonly ChatServiceWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _buyerId = Guid.NewGuid();
    private readonly Guid _sellerId = Guid.NewGuid();
    private readonly Guid _adId = Guid.NewGuid();

    public ChatIntegrationTests(PostgresContainerFixture postgres)
    {
        _factory = new ChatServiceWebAppFactory(postgres);
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", JwtTokenHelper.GenerateToken(_buyerId));
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
    public async Task StartChat_ValidRequest_SavesChatToDb()
    {
        var response = await _client.PostAsJsonAsync("/api/chats", new
        {
            sellerId = _sellerId,
            advertisementId = _adId,
            adTitle = "Тестовий велосипед"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        var chat = await db.Chats.FirstOrDefaultAsync(c =>
            c.BuyerId == _buyerId && c.SellerId == _sellerId);

        chat.Should().NotBeNull();
        chat!.AdvertisementId.Should().Be(_adId);
    }

    [Fact]
    public async Task SendMessage_ValidRequest_SavesMessageToDb()
    {
        var startResp = await _client.PostAsJsonAsync("/api/chats", new
        {
            sellerId = _sellerId,
            advertisementId = _adId,
            adTitle = "Велосипед"
        });
        var chatBody = await startResp.Content.ReadFromJsonAsync<JsonElement>();
        var chatId = chatBody.GetProperty("chatId").GetGuid();

        var msgResp = await _client.PostAsJsonAsync($"/api/chats/{chatId}/messages", new
        {
            content = "Привіт, ще продаєте?"
        });
        msgResp.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        var msg = await db.Messages.FirstOrDefaultAsync(m => m.ChatId == chatId);

        msg.Should().NotBeNull();
        msg!.Content.Should().Be("Привіт, ще продаєте?");
    }
}
