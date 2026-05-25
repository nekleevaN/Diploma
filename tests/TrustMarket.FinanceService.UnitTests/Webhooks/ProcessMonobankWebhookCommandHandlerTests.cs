using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Application.Webhooks;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using Xunit;

namespace TrustMarket.FinanceService.UnitTests.Webhooks;

public class ProcessMonobankWebhookCommandHandlerTests
{
    private readonly IOrderRepository _orderRepo = Substitute.For<IOrderRepository>();
    private readonly IPublishEndpoint _publisher = Substitute.For<IPublishEndpoint>();
    private readonly IMonobankService _monobank = Substitute.For<IMonobankService>();
    private readonly ILogger<ProcessMonobankWebhookCommandHandler> _logger =
        Substitute.For<ILogger<ProcessMonobankWebhookCommandHandler>>();

    private ProcessMonobankWebhookCommandHandler CreateHandler() =>
        new(_orderRepo, _publisher, _monobank, _logger);

    private static MonobankInvoiceStatus MakeWebhook(
        string status,
        string? reference = null,
        DateTime? modified = null,
        string? failureReason = null) => new()
    {
        InvoiceId = "inv_test",
        Status = status,
        Reference = reference ?? Guid.NewGuid().ToString(),
        ModifiedDate = modified ?? DateTime.UtcNow,
        FailureReason = failureReason
    };

    private static Order MakeOrder() =>
        Order.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Ad", 500m);

    [Fact]
    public async Task Handle_InvalidReference_DoesNotQueryRepository()
    {
        var webhook = MakeWebhook("hold", reference: "not-a-valid-guid");

        await CreateHandler().Handle(new ProcessMonobankWebhookCommand(webhook), default);

        await _orderRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OrderNotFound_DoesNotSaveChanges()
    {
        var webhook = MakeWebhook("hold");
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Order?)null);

        await CreateHandler().Handle(new ProcessMonobankWebhookCommand(webhook), default);

        await _orderRepo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_StaleWebhook_SkipsProcessing()
    {
        var order = MakeOrder();
        order.LastWebhookAt = DateTime.UtcNow;
        var staleTime = DateTime.UtcNow.AddMinutes(-10);
        var webhook = MakeWebhook("hold", Guid.NewGuid().ToString(), staleTime);
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns(order);

        await CreateHandler().Handle(new ProcessMonobankWebhookCommand(webhook), default);

        await _orderRepo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HoldStatus_MarksOrderAsPaidAndPublishesOrderPaidEvent()
    {
        var order = MakeOrder();
        var webhook = MakeWebhook("hold");
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns(order);

        await CreateHandler().Handle(new ProcessMonobankWebhookCommand(webhook), default);

        order.Status.Should().Be(OrderStatus.Hold);
        await _publisher.Received(1).Publish(
            Arg.Any<OrderPaidIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SuccessStatus_MarksOrderAsCompletedAndPublishesEvent()
    {
        var order = MakeOrder();
        var webhook = MakeWebhook("success");
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns(order);

        await CreateHandler().Handle(new ProcessMonobankWebhookCommand(webhook), default);

        order.Status.Should().Be(OrderStatus.Completed);
        await _publisher.Received(1).Publish(
            Arg.Any<OrderCompletedIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReversedStatus_MarksOrderAsRefundedAndPublishesCancelledEvent()
    {
        var order = MakeOrder();
        var webhook = MakeWebhook("reversed");
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns(order);

        await CreateHandler().Handle(new ProcessMonobankWebhookCommand(webhook), default);

        order.Status.Should().Be(OrderStatus.Refunded);
        await _publisher.Received(1).Publish(
            Arg.Any<OrderCancelledIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FailureStatus_MarksOrderAsFailedWithReason()
    {
        var order = MakeOrder();
        var webhook = MakeWebhook("failure", failureReason: "card_declined");
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns(order);

        await CreateHandler().Handle(new ProcessMonobankWebhookCommand(webhook), default);

        order.Status.Should().Be(OrderStatus.Failed);
        order.FailureReason.Should().Be("card_declined");
    }

    [Fact]
    public async Task Handle_ExpiredStatus_MarksOrderAsExpired()
    {
        var order = MakeOrder();
        var webhook = MakeWebhook("expired");
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns(order);

        await CreateHandler().Handle(new ProcessMonobankWebhookCommand(webhook), default);

        order.Status.Should().Be(OrderStatus.Expired);
    }

    [Fact]
    public async Task Handle_CreatedStatus_NoStateChangeNoEvent()
    {
        var order = MakeOrder();
        var webhook = MakeWebhook("created");
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns(order);

        await CreateHandler().Handle(new ProcessMonobankWebhookCommand(webhook), default);

        order.Status.Should().Be(OrderStatus.Pending);
        await _publisher.DidNotReceive().Publish(Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidWebhook_AlwaysSavesChanges()
    {
        var order = MakeOrder();
        var webhook = MakeWebhook("hold");
        _orderRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns(order);

        await CreateHandler().Handle(new ProcessMonobankWebhookCommand(webhook), default);

        await _orderRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
