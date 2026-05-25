using FluentAssertions;
using MassTransit;
using NSubstitute;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Application.Orders.Commands;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using Xunit;

namespace TrustMarket.FinanceService.UnitTests.Orders;

public class CancelOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();
    private readonly IMonobankService _monobank = Substitute.For<IMonobankService>();
    private readonly IPublishEndpoint _publisher = Substitute.For<IPublishEndpoint>();

    private CancelOrderCommandHandler CreateHandler() => new(_orderRepo, _monobank, _publisher);

    private static Order MakePendingOrder(Guid buyerId, Guid sellerId) =>
        Order.Create(Guid.NewGuid(), buyerId, sellerId, "Ad", 100m);

    [Fact]
    public async Task Handle_OrderNotFound_ReturnsFailure()
    {
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Order?)null);

        var result = await CreateHandler().Handle(new CancelOrderCommand(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Handle_RequesterIsNeitherBuyerNorSeller_ReturnsFailure()
    {
        var order = MakePendingOrder(Guid.NewGuid(), Guid.NewGuid());
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);

        var result = await CreateHandler().Handle(new CancelOrderCommand(order.Id, Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("заборонено");
    }

    [Fact]
    public async Task Handle_CompletedOrder_ReturnsFailure()
    {
        var buyerId = Guid.NewGuid();
        var order = MakePendingOrder(buyerId, Guid.NewGuid());
        order.MarkAsCompleted(DateTime.UtcNow);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);

        var result = await CreateHandler().Handle(new CancelOrderCommand(order.Id, buyerId), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Неможливо скасувати");
    }

    [Fact]
    public async Task Handle_ExpiredOrder_ReturnsFailure()
    {
        var buyerId = Guid.NewGuid();
        var order = MakePendingOrder(buyerId, Guid.NewGuid());
        order.MarkAsExpired();
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);

        var result = await CreateHandler().Handle(new CancelOrderCommand(order.Id, buyerId), default);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_BuyerCancelsPendingOrderWithoutInvoice_SucceedsWithoutCallingMonobank()
    {
        var buyerId = Guid.NewGuid();
        var order = MakePendingOrder(buyerId, Guid.NewGuid());
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);

        var result = await CreateHandler().Handle(new CancelOrderCommand(order.Id, buyerId), default);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        await _monobank.DidNotReceive().CancelInvoiceAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SellerCancelsHoldOrderWithInvoice_CallsMonobankCancel()
    {
        var sellerId = Guid.NewGuid();
        var order = MakePendingOrder(Guid.NewGuid(), sellerId);
        order.SetInvoiceId("inv_to_cancel");
        order.MarkAsPaid(DateTime.UtcNow);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);

        var result = await CreateHandler().Handle(new CancelOrderCommand(order.Id, sellerId), default);

        result.IsSuccess.Should().BeTrue();
        await _monobank.Received(1).CancelInvoiceAsync("inv_to_cancel", Arg.Any<CancellationToken>());
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_SuccessfulCancellation_PublishesOrderCancelledEvent()
    {
        var buyerId = Guid.NewGuid();
        var order = MakePendingOrder(buyerId, Guid.NewGuid());
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);

        await CreateHandler().Handle(new CancelOrderCommand(order.Id, buyerId), default);

        await _publisher.Received(1).Publish(
            Arg.Any<OrderCancelledIntegrationEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SuccessfulCancellation_SavesChanges()
    {
        var buyerId = Guid.NewGuid();
        var order = MakePendingOrder(buyerId, Guid.NewGuid());
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);

        await CreateHandler().Handle(new CancelOrderCommand(order.Id, buyerId), default);

        await _orderRepo.Received(1).SaveChangesAsync(default);
    }
}
