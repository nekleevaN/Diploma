using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrustMarket.ChatService.Domain.Entities;
using TrustMarket.ChatService.Infrastructure.Persistence;
using TrustMarket.ChatService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using Xunit;

namespace TrustMarket.ChatService.IntegrationTests.Database;

[Collection("ChatService")]
public class ChatDbConstraintTests : IAsyncLifetime
{
    private readonly ChatServiceWebAppFactory _factory;

    public ChatDbConstraintTests(PostgresContainerFixture postgres)
        => _factory = new ChatServiceWebAppFactory(postgres);

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UniqueChat_SameBuyerSellerAd_ViolationThrows()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();

        var buyerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var adId = Guid.NewGuid();

        var chat1 = Chat.Create(buyerId, sellerId, adId, "Товар");
        var chat2 = Chat.Create(buyerId, sellerId, adId, "Товар");

        await db.Chats.AddAsync(chat1);
        await db.SaveChangesAsync();

        await db.Chats.AddAsync(chat2);
        var act = () => db.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task MigrateAsync_CreatesChatsTable()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        var count = await db.Chats.CountAsync();
        count.Should().Be(0);
    }
}
