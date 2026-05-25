using FluentAssertions;
using NSubstitute;
using TrustMarket.FinanceService.Application.Delivery.Commands;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using Xunit;
using DeliveryEntity = global::TrustMarket.FinanceService.Domain.Entities.Delivery;

namespace TrustMarket.FinanceService.UnitTests.Delivery;

public class SetSenderAddressCommandHandlerTests
{
    private readonly IDeliveryRepository _deliveryRepo = Substitute.For<IDeliveryRepository>();
    private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();

    private SetSenderAddressCommandHandler CreateHandler() => new(_deliveryRepo, _orderRepo);

    private static SetSenderAddressCommand ValidCmd(Guid sellerId, Guid orderId) =>
        new(orderId, sellerId, "c2", "Львів", "wh2", "Відд. 2", "Продавець ТОВ", "+380661111111");

    [Fact]
    public async Task Handle_OrderNotFound_ReturnsFailure()
    {
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Order?)null);

        var result = await CreateHandler().Handle(ValidCmd(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Handle_WrongSeller_ReturnsFailure()
    {
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Ad", 100m);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);

        var result = await CreateHandler().Handle(ValidCmd(Guid.NewGuid(), order.Id), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("заборонено");
    }

    [Fact]
    public async Task Handle_NoDeliveryExists_ReturnsFailureAboutBuyer()
    {
        var sellerId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), sellerId, "Ad", 100m);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _deliveryRepo.GetByOrderIdAsync(order.Id, default).Returns((DeliveryEntity?)null);

        var result = await CreateHandler().Handle(ValidCmd(sellerId, order.Id), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("покупець");
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsSenderAddressAndSaves()
    {
        var sellerId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), sellerId, "Ad", 100m);
        var delivery = DeliveryEntity.Create(order.Id, sellerId, order.BuyerId);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _deliveryRepo.GetByOrderIdAsync(order.Id, default).Returns(delivery);

        var result = await CreateHandler().Handle(ValidCmd(sellerId, order.Id), default);

        result.IsSuccess.Should().BeTrue();
        delivery.SenderCityRef.Should().Be("c2");
        delivery.SenderCityName.Should().Be("Львів");
        delivery.SenderName.Should().Be("Продавець ТОВ");
        delivery.SenderPhone.Should().Be("+380661111111");
        _deliveryRepo.Received(1).Update(delivery);
        await _deliveryRepo.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_ValidRequest_DoesNotChangePreviouslySetStatus()
    {
        var sellerId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), sellerId, "Ad", 100m);
        var delivery = DeliveryEntity.Create(order.Id, sellerId, order.BuyerId);
        delivery.SetRecipientAddress("c1", "Kyiv", "wh1", "Відд. 1", "Іван", "+380501234567");
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _deliveryRepo.GetByOrderIdAsync(order.Id, default).Returns(delivery);

        await CreateHandler().Handle(ValidCmd(sellerId, order.Id), default);

        delivery.Status.Should().Be(DeliveryStatus.AddressSet);
    }
}
