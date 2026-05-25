using FluentAssertions;
using MassTransit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Application.Orders.Commands;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using Xunit;

namespace TrustMarket.FinanceService.UnitTests.Orders;

public class FinalizeOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();
    private readonly IMonobankService _monobank = Substitute.For<IMonobankService>();
    private readonly IPublishEndpoint _publisher = Substitute.For<IPublishEndpoint>();

    private FinalizeOrderCommandHandler CreateHandler() => new(_orderRepo, _monobank, _publisher);

    private static Order MakeHoldOrder(Guid sellerId, string invoiceId = "inv_hold")
    {
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), sellerId, "Ad", 1000m);
        order.SetInvoiceId(invoiceId);
        order.MarkAsPaid(DateTime.UtcNow);
        return order;
    }

    [Fact]
    public async Task Handle_OrderNotFound_ReturnsFailure()
    {
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Order?)null);

        var result = await CreateHandler().Handle(new FinalizeOrderCommand(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Handle_WrongSeller_ReturnsFailure()
    {
        var order = MakeHoldOrder(Guid.NewGuid());
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);

        var result = await CreateHandler().Handle(new FinalizeOrderCommand(order.Id, Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("заборонено");
    }

    [Fact]
    public async Task Handle_OrderNotInHoldStatus_ReturnsFailure()
    {
        var sellerId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), sellerId, "Ad", 100m);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);

        var result = await CreateHandler().Handle(new FinalizeOrderCommand(order.Id, sellerId), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Неможливо підтвердити");
    }

    [Fact]
    public async Task Handle_MissingInvoiceId_ReturnsFailure()
    {
        var sellerId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), sellerId, "Ad", 100m);
        order.MarkAsPaid(DateTime.UtcNow); // Hold, but no InvoiceId
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);

        var result = await CreateHandler().Handle(new FinalizeOrderCommand(order.Id, sellerId), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("InvoiceId");
    }

    [Fact]
    public async Task Handle_FinalizeSucceeds_CompletesOrderAndPublishesEvent()
    {
        var sellerId = Guid.NewGuid();
        var order = MakeHoldOrder(sellerId);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _monobank.FinalizeHoldAsync(Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await CreateHandler().Handle(new FinalizeOrderCommand(order.Id, sellerId), default);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Completed);
        await _publisher.Received(1).Publish(
            Arg.Any<OrderCompletedIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FinalizeFails_MonobankAlreadySuccess_CompletesOrderAnyway()
    {
        var sellerId = Guid.NewGuid();
        var order = MakeHoldOrder(sellerId);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _monobank.FinalizeHoldAsync(Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _monobank.GetInvoiceStatusAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new MonobankInvoiceStatus { Status = "success" });

        var result = await CreateHandler().Handle(new FinalizeOrderCommand(order.Id, sellerId), default);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Fact]
    public async Task Handle_FinalizeFails_MonobankStillHold_ReturnsFailure()
    {
        var sellerId = Guid.NewGuid();
        var order = MakeHoldOrder(sellerId);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _monobank.FinalizeHoldAsync(Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _monobank.GetInvoiceStatusAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new MonobankInvoiceStatus { Status = "hold" });

        var result = await CreateHandler().Handle(new FinalizeOrderCommand(order.Id, sellerId), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Помилка списання");
    }

    [Fact]
    public async Task Handle_FinalizeFails_MonobankStatusCheckThrows_ReturnsFailure()
    {
        var sellerId = Guid.NewGuid();
        var order = MakeHoldOrder(sellerId);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _monobank.FinalizeHoldAsync(Arg.Any<string>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _monobank.GetInvoiceStatusAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsForAnyArgs(new HttpRequestException("timeout"));

        var result = await CreateHandler().Handle(new FinalizeOrderCommand(order.Id, sellerId), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("зв'язку");
    }
}
