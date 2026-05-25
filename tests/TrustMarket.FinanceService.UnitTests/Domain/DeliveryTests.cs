using FluentAssertions;
using TrustMarket.FinanceService.Domain.Entities;
using Xunit;
using DeliveryEntity = global::TrustMarket.FinanceService.Domain.Entities.Delivery;

namespace TrustMarket.FinanceService.UnitTests.Domain;

public class DeliveryTests
{
    private static DeliveryEntity Make() =>
        DeliveryEntity.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Create_SetsInitialStatusToPendingAddress()
    {
        var delivery = Make();

        delivery.Status.Should().Be(DeliveryStatus.PendingAddress);
        delivery.TTN.Should().BeNull();
    }

    [Fact]
    public void Create_AssignsCorrectParticipants()
    {
        var orderId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();

        var delivery = DeliveryEntity.Create(orderId, sellerId, buyerId);

        delivery.OrderId.Should().Be(orderId);
        delivery.SellerId.Should().Be(sellerId);
        delivery.BuyerId.Should().Be(buyerId);
    }

    [Fact]
    public void SetRecipientAddress_TransitionsToAddressSet()
    {
        var delivery = Make();

        delivery.SetRecipientAddress("city-ref", "Київ", "wh-ref", "Відд. 1", "Іван Іваненко", "+380501234567");

        delivery.Status.Should().Be(DeliveryStatus.AddressSet);
        delivery.RecipientCityRef.Should().Be("city-ref");
        delivery.RecipientCityName.Should().Be("Київ");
        delivery.RecipientWarehouseRef.Should().Be("wh-ref");
        delivery.RecipientWarehouseAddress.Should().Be("Відд. 1");
        delivery.RecipientName.Should().Be("Іван Іваненко");
        delivery.RecipientPhone.Should().Be("+380501234567");
    }

    [Fact]
    public void SetSenderAddress_StoresFieldsWithoutChangingStatus()
    {
        var delivery = Make();

        delivery.SetSenderAddress("s-city", "Львів", "s-wh", "Відд. 2", "Продавець", "+380661111111");

        delivery.Status.Should().Be(DeliveryStatus.PendingAddress);
        delivery.SenderCityRef.Should().Be("s-city");
        delivery.SenderName.Should().Be("Продавець");
        delivery.SenderPhone.Should().Be("+380661111111");
    }

    [Fact]
    public void SetTTN_TransitionsToTTNCreated()
    {
        var delivery = Make();

        delivery.SetTTN("20450000000001");

        delivery.Status.Should().Be(DeliveryStatus.TTNCreated);
        delivery.TTN.Should().Be("20450000000001");
    }

    [Theory]
    [InlineData("5", DeliveryStatus.AtWarehouse)]
    [InlineData("6", DeliveryStatus.InTransit)]
    [InlineData("7", DeliveryStatus.Arrived)]
    [InlineData("9", DeliveryStatus.Received)]
    [InlineData("14", DeliveryStatus.Returned)]
    public void UpdateTracking_MapsNovaPoshtaCodeToStatus(string code, DeliveryStatus expected)
    {
        var delivery = Make();

        delivery.UpdateTracking(code, "Опис статусу");

        delivery.Status.Should().Be(expected);
        delivery.TrackingStatus.Should().Be(code);
        delivery.TrackingStatusDescription.Should().Be("Опис статусу");
    }

    [Fact]
    public void UpdateTracking_ReceivedStatus_SetsActualDeliveryDate()
    {
        var delivery = Make();

        delivery.UpdateTracking("9", "Отримано одержувачем");

        delivery.ActualDeliveryDate.Should().NotBeNull();
    }

    [Fact]
    public void UpdateTracking_UnknownCode_PreservesCurrentStatus()
    {
        var delivery = Make();

        delivery.UpdateTracking("99", "Невідомий статус");

        delivery.Status.Should().Be(DeliveryStatus.PendingAddress);
    }

    [Fact]
    public void UpdateTracking_WithEstimatedDate_StoresIt()
    {
        var delivery = Make();
        var estimated = new DateTime(2025, 2, 20, 0, 0, 0, DateTimeKind.Utc);

        delivery.UpdateTracking("6", "В дорозі", estimated);

        delivery.EstimatedDeliveryDate.Should().Be(estimated);
    }

    [Fact]
    public void IsReadyForTTN_FalseWhenNoWarehouseRefsSet()
    {
        Make().IsReadyForTTN.Should().BeFalse();
    }

    [Fact]
    public void IsReadyForTTN_FalseWhenOnlyRecipientWarehouseSet()
    {
        var delivery = Make();
        delivery.SetRecipientAddress("c1", "Київ", "wh1", "Відд. 1", "Іван", "+380501234567");

        delivery.IsReadyForTTN.Should().BeFalse();
    }

    [Fact]
    public void IsReadyForTTN_TrueWhenBothWarehouseRefsSet()
    {
        var delivery = Make();
        delivery.SetRecipientAddress("c1", "Київ", "wh1", "Відд. 1", "Іван", "+380501234567");
        delivery.SetSenderAddress("c2", "Львів", "wh2", "Відд. 2", "Продавець", "+380661111111");

        delivery.IsReadyForTTN.Should().BeTrue();
    }
}
