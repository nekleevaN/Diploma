using FluentAssertions;
using NSubstitute;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Application.Orders.Commands;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using Xunit;

namespace TrustMarket.FinanceService.UnitTests.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();
    private readonly IMonobankService _monobank = Substitute.For<IMonobankService>();
    private readonly Microsoft.Extensions.Configuration.IConfiguration _config =
        Substitute.For<Microsoft.Extensions.Configuration.IConfiguration>();

    public CreateOrderCommandHandlerTests()
    {
        _config["Monobank:WebhookBaseUrl"].Returns((string?)null);
        _config["Monobank:RedirectBaseUrl"].Returns("http://localhost:3000");
    }

    private CreateOrderCommandHandler CreateHandler() => new(_orderRepo, _monobank, _config);

    private static CreateOrderCommand ValidCommand(Guid? buyerId = null, Guid? sellerId = null) =>
        new(Guid.NewGuid(), buyerId ?? Guid.NewGuid(), sellerId ?? Guid.NewGuid(), "Телефон Samsung", 5000m);

    [Fact]
    public async Task Handle_BuyerIsSeller_ReturnsFailure()
    {
        var sameId = Guid.NewGuid();
        var cmd = new CreateOrderCommand(Guid.NewGuid(), sameId, sameId, "Ad", 100m);

        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("власне");
    }

    [Fact]
    public async Task Handle_ExistingPendingOrder_ReturnsFailure()
    {
        var cmd = ValidCommand();
        var existing = Order.Create(cmd.AdvertisementId, cmd.BuyerId, cmd.SellerId, "Ad", 100m);
        _orderRepo.GetByAdvertisementAndBuyerAsync(cmd.AdvertisementId, cmd.BuyerId, default)
            .Returns(existing);

        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("вже існує");
    }

    [Fact]
    public async Task Handle_ExistingHoldOrder_ReturnsFailure()
    {
        var cmd = ValidCommand();
        var existing = Order.Create(cmd.AdvertisementId, cmd.BuyerId, cmd.SellerId, "Ad", 100m);
        existing.MarkAsPaid(DateTime.UtcNow);
        _orderRepo.GetByAdvertisementAndBuyerAsync(cmd.AdvertisementId, cmd.BuyerId, default)
            .Returns(existing);

        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesOrderAndReturnsPageUrl()
    {
        var cmd = ValidCommand();
        _orderRepo.GetByAdvertisementAndBuyerAsync(default, default, default)
            .ReturnsForAnyArgs((Order?)null);
        _monobank.CreateHoldInvoiceAsync(default, default!, default!, default!, default!, default, default)
            .ReturnsForAnyArgs(new MonobankInvoiceResult("inv_1", "https://pay.monobank.ua/inv_1"));

        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PageUrl.Should().Be("https://pay.monobank.ua/inv_1");
        result.Value.OrderId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsOrderTwice()
    {
        var cmd = ValidCommand();
        _orderRepo.GetByAdvertisementAndBuyerAsync(default, default, default)
            .ReturnsForAnyArgs((Order?)null);
        _monobank.CreateHoldInvoiceAsync(default, default!, default!, default!, default!, default, default)
            .ReturnsForAnyArgs(new MonobankInvoiceResult("inv_x", "https://pay.monobank.ua/inv_x"));

        await CreateHandler().Handle(cmd, default);

        await _orderRepo.Received(2).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsInvoiceIdOnOrder()
    {
        var cmd = ValidCommand();
        _orderRepo.GetByAdvertisementAndBuyerAsync(default, default, default)
            .ReturnsForAnyArgs((Order?)null);
        _monobank.CreateHoldInvoiceAsync(default, default!, default!, default!, default!, default, default)
            .ReturnsForAnyArgs(new MonobankInvoiceResult("inv_42", "https://pay.monobank.ua"));

        await CreateHandler().Handle(cmd, default);

        _orderRepo.Received(1).Update(Arg.Is<Order>(o => o.InvoiceId == "inv_42"));
    }

    [Fact]
    public async Task Handle_ExistingCancelledOrder_AllowsNewOrder()
    {
        var cmd = ValidCommand();
        var old = Order.Create(cmd.AdvertisementId, cmd.BuyerId, cmd.SellerId, "Ad", 100m);
        old.MarkAsCancelled();
        _orderRepo.GetByAdvertisementAndBuyerAsync(cmd.AdvertisementId, cmd.BuyerId, default)
            .Returns(old);
        _monobank.CreateHoldInvoiceAsync(default, default!, default!, default!, default!, default, default)
            .ReturnsForAnyArgs(new MonobankInvoiceResult("inv_new", "https://pay.monobank.ua"));

        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
    }
}
