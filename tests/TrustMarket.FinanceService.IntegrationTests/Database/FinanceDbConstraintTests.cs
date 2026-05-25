using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Infrastructure.Persistence;
using TrustMarket.FinanceService.IntegrationTests.Factories;
using TrustMarket.TestInfrastructure.Fixtures;
using Xunit;

namespace TrustMarket.FinanceService.IntegrationTests.Database;

[Collection("FinanceService")]
public class FinanceDbConstraintTests : IAsyncLifetime
{
    private readonly FinanceServiceWebAppFactory _factory;

    public FinanceDbConstraintTests(PostgresContainerFixture postgres)
        => _factory = new FinanceServiceWebAppFactory(postgres);

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task MigrateAsync_CreatesOrdersTable()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
        var count = await db.Orders.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task Order_CanBeSavedAndRetrieved()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();

        var order = Order.Create(
            advertisementId: Guid.NewGuid(),
            buyerId: Guid.NewGuid(),
            sellerId: Guid.NewGuid(),
            adTitle: "Тест",
            amount: 500m);
        order.SetInvoiceId("inv-001");

        await db.Orders.AddAsync(order);
        await db.SaveChangesAsync();

        var saved = await db.Orders.FindAsync(order.Id);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(OrderStatus.Pending);
        saved.InvoiceId.Should().Be("inv-001");
    }

    [Fact]
    public async Task Delivery_FkToOrder_IsEnforced()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();

        var delivery = Delivery.Create(
            orderId: Guid.NewGuid(), // non-existent order
            sellerId: Guid.NewGuid(),
            buyerId: Guid.NewGuid());

        await db.Deliveries.AddAsync(delivery);
        var act = () => db.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
