using FluentAssertions;
using NSubstitute;
using TrustMarket.FinanceService.Application.Delivery.Commands;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using Xunit;
using DeliveryEntity = global::TrustMarket.FinanceService.Domain.Entities.Delivery;

namespace TrustMarket.FinanceService.UnitTests.Delivery;

public class SetRecipientAddressCommandHandlerTests
{
    private readonly IDeliveryRepository _deliveryRepo = Substitute.For<IDeliveryRepository>();
    private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();

    private SetRecipientAddressCommandHandler CreateHandler() => new(_deliveryRepo, _orderRepo);

    private static SetRecipientAddressCommand ValidCmd(Guid buyerId, Guid orderId) =>
        new(orderId, buyerId, "c1", "Київ", "wh1", "Відд. 1", "Іван Іваненко", "+380501234567");

    [Fact]
    public async Task Handle_OrderNotFound_ReturnsFailure()
    {
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Order?)null);

        var result = await CreateHandler().Handle(ValidCmd(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Handle_WrongBuyer_ReturnsFailure()
    {
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Ad", 100m);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);

        var result = await CreateHandler().Handle(ValidCmd(Guid.NewGuid(), order.Id), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("заборонено");
    }

    [Fact]
    public async Task Handle_NoExistingDelivery_CreatesNewDeliveryWithAddressSet()
    {
        var buyerId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), buyerId, Guid.NewGuid(), "Ad", 100m);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _deliveryRepo.GetByOrderIdAsync(order.Id, default).Returns((DeliveryEntity?)null);

        var result = await CreateHandler().Handle(ValidCmd(buyerId, order.Id), default);

        result.IsSuccess.Should().BeTrue();
        await _deliveryRepo.Received(1).AddAsync(
            Arg.Is<DeliveryEntity>(d =>
                d.Status == DeliveryStatus.AddressSet &&
                d.RecipientName == "Іван Іваненко" &&
                d.RecipientPhone == "+380501234567"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingDelivery_UpdatesAddressInPlace()
    {
        var buyerId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), buyerId, Guid.NewGuid(), "Ad", 100m);
        var existing = DeliveryEntity.Create(order.Id, order.SellerId, buyerId);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _deliveryRepo.GetByOrderIdAsync(order.Id, default).Returns(existing);

        var result = await CreateHandler().Handle(ValidCmd(buyerId, order.Id), default);

        result.IsSuccess.Should().BeTrue();
        existing.Status.Should().Be(DeliveryStatus.AddressSet);
        existing.RecipientName.Should().Be("Іван Іваненко");
        _deliveryRepo.Received(1).Update(existing);
        await _deliveryRepo.DidNotReceive().AddAsync(Arg.Any<DeliveryEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingDelivery_ReturnsThatDeliveryId()
    {
        var buyerId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), buyerId, Guid.NewGuid(), "Ad", 100m);
        var existing = DeliveryEntity.Create(order.Id, order.SellerId, buyerId);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _deliveryRepo.GetByOrderIdAsync(order.Id, default).Returns(existing);

        var result = await CreateHandler().Handle(ValidCmd(buyerId, order.Id), default);

        result.Value.Should().Be(existing.Id);
    }
}
