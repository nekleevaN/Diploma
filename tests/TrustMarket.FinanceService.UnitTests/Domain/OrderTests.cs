using FluentAssertions;
using TrustMarket.FinanceService.Domain.Entities;
using Xunit;

namespace TrustMarket.FinanceService.UnitTests.Domain;

public class OrderTests
{
    private static Order Make() =>
        Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Тестове оголошення", 1000m);

    [Fact]
    public void Create_SetsInitialStatusToPending()
    {
        var order = Make();

        order.Status.Should().Be(OrderStatus.Pending);
        order.InvoiceId.Should().BeNull();
        order.FailureReason.Should().BeNull();
    }

    [Fact]
    public void Create_AssignsCorrectFields()
    {
        var adId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();

        var order = Order.Create(adId, buyerId, sellerId, "Ad title", 500m);

        order.AdvertisementId.Should().Be(adId);
        order.BuyerId.Should().Be(buyerId);
        order.SellerId.Should().Be(sellerId);
        order.AdTitle.Should().Be("Ad title");
        order.Amount.Should().Be(500m);
    }

    [Fact]
    public void SetInvoiceId_AssignsInvoiceId()
    {
        var order = Make();
        order.SetInvoiceId("inv_abc123");
        order.InvoiceId.Should().Be("inv_abc123");
    }

    [Fact]
    public void MarkAsPaid_TransitionsToPendingToHold()
    {
        var order = Make();
        var at = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        order.MarkAsPaid(at);

        order.Status.Should().Be(OrderStatus.Hold);
        order.LastWebhookAt.Should().Be(at);
    }

    [Fact]
    public void MarkAsCompleted_SetsCompletedStatusAndTimestamp()
    {
        var order = Make();
        var at = DateTime.UtcNow;

        order.MarkAsCompleted(at);

        order.Status.Should().Be(OrderStatus.Completed);
        order.LastWebhookAt.Should().Be(at);
    }

    [Fact]
    public void MarkAsRefunded_SetsRefundedStatus()
    {
        var order = Make();
        var at = DateTime.UtcNow;

        order.MarkAsRefunded(at);

        order.Status.Should().Be(OrderStatus.Refunded);
        order.LastWebhookAt.Should().Be(at);
    }

    [Fact]
    public void MarkAsFailed_SetsFailedStatusAndStoresReason()
    {
        var order = Make();

        order.MarkAsFailed("card_declined");

        order.Status.Should().Be(OrderStatus.Failed);
        order.FailureReason.Should().Be("card_declined");
    }

    [Fact]
    public void MarkAsExpired_SetsExpiredStatus()
    {
        var order = Make();
        order.MarkAsExpired();
        order.Status.Should().Be(OrderStatus.Expired);
    }

    [Fact]
    public void MarkAsCancelled_SetsCancelledStatus()
    {
        var order = Make();
        order.MarkAsCancelled();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void MarkAsPaid_ThenMarkAsCompleted_BothTransitionsApply()
    {
        var order = Make();
        order.MarkAsPaid(DateTime.UtcNow);
        order.Status.Should().Be(OrderStatus.Hold);

        order.MarkAsCompleted(DateTime.UtcNow);
        order.Status.Should().Be(OrderStatus.Completed);
    }
}
