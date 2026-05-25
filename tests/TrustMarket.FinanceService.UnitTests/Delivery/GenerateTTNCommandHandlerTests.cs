using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Application.Delivery.Commands;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using Xunit;
using DeliveryEntity = global::TrustMarket.FinanceService.Domain.Entities.Delivery;

namespace TrustMarket.FinanceService.UnitTests.Delivery;

public class GenerateTTNCommandHandlerTests
{
    private readonly IDeliveryRepository _deliveryRepo = Substitute.For<IDeliveryRepository>();
    private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();
    private readonly INovaPoshtaService _np = Substitute.For<INovaPoshtaService>();
    private readonly ILogger<GenerateTTNCommandHandler> _logger =
        Substitute.For<ILogger<GenerateTTNCommandHandler>>();

    private GenerateTTNCommandHandler CreateHandler() =>
        new(_deliveryRepo, _orderRepo, _np, _logger);

    private static (Order order, DeliveryEntity delivery) MakeReadyPair(Guid sellerId)
    {
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), sellerId, "Ad", 500m);
        var delivery = DeliveryEntity.Create(order.Id, sellerId, order.BuyerId);
        delivery.SetRecipientAddress("c1", "Київ", "wh1", "Відд. 1", "Іван", "+380501234567");
        delivery.SetSenderAddress("c2", "Львів", "wh2", "Відд. 2", "Продавець", "+380661111111");
        return (order, delivery);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ReturnsFailure()
    {
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Order?)null);

        var result = await CreateHandler().Handle(new GenerateTTNCommand(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Handle_WrongSeller_ReturnsFailure()
    {
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Ad", 100m);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);

        var result = await CreateHandler().Handle(new GenerateTTNCommand(order.Id, Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("заборонено");
    }

    [Fact]
    public async Task Handle_NoDelivery_ReturnsFailure()
    {
        var sellerId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), sellerId, "Ad", 100m);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _deliveryRepo.GetByOrderIdAsync(order.Id, default).Returns((DeliveryEntity?)null);

        var result = await CreateHandler().Handle(new GenerateTTNCommand(order.Id, sellerId), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("адрес");
    }

    [Fact]
    public async Task Handle_DeliveryMissingSenderWarehouse_ReturnsFailure()
    {
        var sellerId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid(), Guid.NewGuid(), sellerId, "Ad", 100m);
        var delivery = DeliveryEntity.Create(order.Id, sellerId, order.BuyerId);
        delivery.SetRecipientAddress("c1", "Київ", "wh1", "Відд. 1", "Іван", "+380501234567");
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _deliveryRepo.GetByOrderIdAsync(order.Id, default).Returns(delivery);

        var result = await CreateHandler().Handle(new GenerateTTNCommand(order.Id, sellerId), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("відділення відправника");
    }

    [Fact]
    public async Task Handle_TTNAlreadyExists_ReturnsExistingTTNWithoutCallingNovaPoshta()
    {
        var sellerId = Guid.NewGuid();
        var (order, delivery) = MakeReadyPair(sellerId);
        delivery.SetTTN("20450000111222");
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _deliveryRepo.GetByOrderIdAsync(order.Id, default).Returns(delivery);

        var result = await CreateHandler().Handle(new GenerateTTNCommand(order.Id, sellerId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("20450000111222");
        await _np.DidNotReceive()
            .CreateWaybillAsync(Arg.Any<CreateWaybillRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HappyPath_CreatesWaybillSetsDeliveryTTNAndStatus()
    {
        var sellerId = Guid.NewGuid();
        var (order, delivery) = MakeReadyPair(sellerId);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _deliveryRepo.GetByOrderIdAsync(order.Id, default).Returns(delivery);
        _np.CreateWaybillAsync(Arg.Any<CreateWaybillRequest>(), Arg.Any<CancellationToken>())
            .Returns("20450099887766");

        var result = await CreateHandler().Handle(new GenerateTTNCommand(order.Id, sellerId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("20450099887766");
        delivery.TTN.Should().Be("20450099887766");
        delivery.Status.Should().Be(DeliveryStatus.TTNCreated);
        _deliveryRepo.Received(1).Update(delivery);
    }

    [Fact]
    public async Task Handle_NovaPoshtaThrowsDataInvalid_UsesFallbackTTN()
    {
        var sellerId = Guid.NewGuid();
        var (order, delivery) = MakeReadyPair(sellerId);
        _orderRepo.GetByIdAsync(order.Id, default).Returns(order);
        _deliveryRepo.GetByOrderIdAsync(order.Id, default).Returns(delivery);
        _np.CreateWaybillAsync(Arg.Any<CreateWaybillRequest>(), Arg.Any<CancellationToken>())
            .ThrowsForAnyArgs(new InvalidOperationException("Data is invalid"));

        var result = await CreateHandler().Handle(new GenerateTTNCommand(order.Id, sellerId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().StartWith("20");
        delivery.TTN.Should().NotBeNullOrEmpty();
    }
}
