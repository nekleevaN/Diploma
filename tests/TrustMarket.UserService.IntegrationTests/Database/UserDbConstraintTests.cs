using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using TrustMarket.TestInfrastructure.Fixtures;
using TrustMarket.UserService.Domain.Entities;
using TrustMarket.UserService.Infrastructure.Persistence;
using TrustMarket.UserService.IntegrationTests.Factories;
using Xunit;

namespace TrustMarket.UserService.IntegrationTests.Database;

[Collection("UserService")]
public class UserDbConstraintTests : IAsyncLifetime
{
    private readonly UserServiceWebAppFactory _factory;

    public UserDbConstraintTests(PostgresContainerFixture postgres)
        => _factory = new UserServiceWebAppFactory(postgres);

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UniqueEmailConstraint_ViolationThrows()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        var user1 = User.Create("dup@example.com", "user1", "A", "B", "hash");
        var user2 = User.Create("dup@example.com", "user2", "C", "D", "hash2");

        await db.Users.AddAsync(user1);
        await db.SaveChangesAsync();

        await db.Users.AddAsync(user2);
        var act = () => db.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task UniqueUsernameConstraint_ViolationThrows()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        var user1 = User.Create("a@example.com", "sameusername", "A", "B", "hash");
        var user2 = User.Create("b@example.com", "sameusername", "C", "D", "hash2");

        await db.Users.AddAsync(user1);
        await db.SaveChangesAsync();

        await db.Users.AddAsync(user2);
        var act = () => db.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task MigrateAsync_CreatesUsersTable()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        var count = await db.Users.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task CascadeDelete_RemovesBadgesWhenUserDeleted()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();

        var user = User.Create("cascade@example.com", "cascadeuser", "A", "B", "hash");
        user.VerifyViaDiia("Тест Тестовий", "1234567890");
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        var badgeCount = await db.VerificationBadges.CountAsync(b => b.UserId == user.Id);
        badgeCount.Should().Be(0);
    }
}
