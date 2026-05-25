using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Application.Orders.Commands;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using Xunit;
using DeliveryEntity = global::TrustMarket.FinanceService.Domain.Entities.Delivery;

namespace TrustMarket.FinanceService.UnitTests.Orders;

public class CreateCheckoutOrderCommandHandlerTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IDeliveryRepository _deliveries = Substitute.For<IDeliveryRepository>();
    private readonly IMonobankService _monobank = Substitute.For<IMonobankService>();
    private readonly ICatalogServiceClient _catalog = Substitute.For<ICatalogServiceClient>();
    private readonly Microsoft.Extensions.Configuration.IConfiguration _config =
        Substitute.For<Microsoft.Extensions.Configuration.IConfiguration>();

    public CreateCheckoutOrderCommandHandlerTests()
    {
        _config["Monobank:WebhookBaseUrl"].Returns((string?)null);
        _config["Monobank:RedirectBaseUrl"].Returns("http://localhost:3000");
        _config["Platform:FeePercent"].Returns("5");
    }

    private CreateCheckoutOrderCommandHandler CreateHandler() =>
        new(_orders, _deliveries, _monobank, _catalog, _config);

    private static CreateCheckoutOrderCommand ValidCommand(Guid? buyerId = null) => new(
        AdvertisementId: Guid.NewGuid(),
        BuyerId: buyerId ?? Guid.NewGuid(),
        Amount: 2000m,
        RecipientCityRef: "city-1", RecipientCityName: "Київ",
        RecipientWarehouseRef: "wh-1", RecipientWarehouseAddress: "Відд. 1",
        RecipientFirstName: "Іван", RecipientLastName: "Іваненко",
        RecipientPhone: "+380501234567");

    private static AdReservationResult SuccessReservation(string? subMerchantId = "sub_merchant_123") =>
        new(true, false, null, new AdReservation(Guid.NewGuid(), "Телефон Samsung", 2000m, subMerchantId));

    [Fact]
    public async Task Handle_ReservationFails_ReturnsFailure()
    {
        var cmd = ValidCommand();
        _catalog.ReserveAdvertisementAsync(cmd.AdvertisementId, default)
            .ReturnsForAnyArgs(new AdReservationResult(false, false, "Оголошення не активне", null));

        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не активне");
    }

    [Fact]
    public async Task Handle_ReservationConflict_ReturnsConflictPrefixedError()
    {
        var cmd = ValidCommand();
        _catalog.ReserveAdvertisementAsync(cmd.AdvertisementId, default)
            .ReturnsForAnyArgs(new AdReservationResult(false, true, "Вже зарезервовано", null));

        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().StartWith("CONFLICT:");
    }

    [Fact]
    public async Task Handle_SellerHasNoSubMerchantId_UnreservesAndReturnsPayoutError()
    {
        var cmd = ValidCommand();
        _catalog.ReserveAdvertisementAsync(cmd.AdvertisementId, default)
            .ReturnsForAnyArgs(SuccessReservation(subMerchantId: null));

        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("SELLER_NO_PAYOUT");
        await _catalog.Received(1).UnreserveAdvertisementAsync(
            cmd.AdvertisementId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ActiveOrderAlreadyExists_UnreservesAndReturnsFailure()
    {
        var buyerId = Guid.NewGuid();
        var cmd = ValidCommand(buyerId);
        _catalog.ReserveAdvertisementAsync(cmd.AdvertisementId, default)
            .ReturnsForAnyArgs(SuccessReservation());
        var existing = Order.Create(cmd.AdvertisementId, buyerId, Guid.NewGuid(), "Ad", 100m);
        _orders.GetByAdvertisementAndBuyerAsync(cmd.AdvertisementId, buyerId, default)
            .ReturnsForAnyArgs(existing);

        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("вже існує");
        await _catalog.Received(1).UnreserveAdvertisementAsync(
            cmd.AdvertisementId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MonobankThrows_UnreservesAndReturnsUnavailableError()
    {
        var cmd = ValidCommand();
        _catalog.ReserveAdvertisementAsync(cmd.AdvertisementId, default)
            .ReturnsForAnyArgs(SuccessReservation());
        _orders.GetByAdvertisementAndBuyerAsync(default, default, default)
            .ReturnsForAnyArgs((Order?)null);
        _monobank.CreateHoldInvoiceAsync(default, default!, default!, default!, default!, default, default)
            .ThrowsForAnyArgs(new HttpRequestException("Monobank unavailable"));

        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("тимчасово недоступна");
        await _catalog.Received(1).UnreserveAdvertisementAsync(
            cmd.AdvertisementId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HappyPath_ReturnsPageUrl()
    {
        var cmd = ValidCommand();
        _catalog.ReserveAdvertisementAsync(cmd.AdvertisementId, default)
            .ReturnsForAnyArgs(SuccessReservation());
        _orders.GetByAdvertisementAndBuyerAsync(default, default, default)
            .ReturnsForAnyArgs((Order?)null);
        _monobank.CreateHoldInvoiceAsync(default, default!, default!, default!, default!, default, default)
            .ReturnsForAnyArgs(new MonobankInvoiceResult("inv_checkout", "https://pay.mono/checkout"));

        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PageUrl.Should().Be("https://pay.mono/checkout");
    }

    [Fact]
    public async Task Handle_HappyPath_CreatesDeliveryWithRecipientAddress()
    {
        var cmd = ValidCommand();
        _catalog.ReserveAdvertisementAsync(cmd.AdvertisementId, default)
            .ReturnsForAnyArgs(SuccessReservation());
        _orders.GetByAdvertisementAndBuyerAsync(default, default, default)
            .ReturnsForAnyArgs((Order?)null);
        _monobank.CreateHoldInvoiceAsync(default, default!, default!, default!, default!, default, default)
            .ReturnsForAnyArgs(new MonobankInvoiceResult("inv_x", "https://pay.mono"));

        await CreateHandler().Handle(cmd, default);

        await _deliveries.Received(1).AddAsync(
            Arg.Is<DeliveryEntity>(d =>
                d.Status == DeliveryStatus.AddressSet &&
                d.RecipientCityRef == "city-1" &&
                d.RecipientPhone == "+380501234567"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HappyPath_SetsInvoiceIdOnOrder()
    {
        var cmd = ValidCommand();
        _catalog.ReserveAdvertisementAsync(cmd.AdvertisementId, default)
            .ReturnsForAnyArgs(SuccessReservation());
        _orders.GetByAdvertisementAndBuyerAsync(default, default, default)
            .ReturnsForAnyArgs((Order?)null);
        _monobank.CreateHoldInvoiceAsync(default, default!, default!, default!, default!, default, default)
            .ReturnsForAnyArgs(new MonobankInvoiceResult("inv_checkout_final", "https://pay.mono"));

        await CreateHandler().Handle(cmd, default);

        _orders.Received(1).Update(Arg.Is<Order>(o => o.InvoiceId == "inv_checkout_final"));
    }
}
