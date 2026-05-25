using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrustMarket.ReviewService.Domain.Entities;
using TrustMarket.ReviewService.Infrastructure.Persistence;
using TrustMarket.ReviewService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using Xunit;

namespace TrustMarket.ReviewService.IntegrationTests.Database;

[Collection("ReviewService")]
public class ReviewDbConstraintTests : IAsyncLifetime
{
    private readonly ReviewServiceWebAppFactory _factory;

    public ReviewDbConstraintTests(PostgresContainerFixture postgres)
        => _factory = new ReviewServiceWebAppFactory(postgres);

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task UniqueOrderIdReviewType_ViolationThrows()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();

        var orderId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var revieweeId = Guid.NewGuid();

        var review1 = Review.CreatePlaceholder(orderId, reviewerId, revieweeId, "Покупець", ReviewType.BuyerToSeller);
        var review2 = Review.CreatePlaceholder(orderId, reviewerId, revieweeId, "Покупець", ReviewType.BuyerToSeller);

        await db.Reviews.AddAsync(review1);
        await db.SaveChangesAsync();

        await db.Reviews.AddAsync(review2);
        var act = () => db.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task MigrateAsync_CreatesReviewsTable()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();
        var count = await db.Reviews.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task Review_ExpiresAt_IsPersistedCorrectly()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReviewDbContext>();

        var review = Review.CreatePlaceholder(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Покупець", ReviewType.BuyerToSeller);

        await db.Reviews.AddAsync(review);
        await db.SaveChangesAsync();

        var saved = await db.Reviews.FindAsync(review.Id);
        saved!.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }
}
