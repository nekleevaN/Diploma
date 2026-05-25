using MediatR;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.FinanceService.Application.Delivery.Commands;

public record SetRecipientAddressCommand(
    Guid OrderId,
    Guid BuyerId,
    string CityRef,
    string CityName,
    string WarehouseRef,
    string WarehouseAddress,
    string RecipientName,
    string RecipientPhone) : IRequest<Result<Guid>>;

public class SetRecipientAddressCommandHandler : IRequestHandler<SetRecipientAddressCommand, Result<Guid>>
{
    private readonly IDeliveryRepository _deliveryRepo;
    private readonly IOrderRepository _orderRepo;

    public SetRecipientAddressCommandHandler(IDeliveryRepository deliveryRepo, IOrderRepository orderRepo)
    {
        _deliveryRepo = deliveryRepo;
        _orderRepo = orderRepo;
    }

    public async Task<Result<Guid>> Handle(SetRecipientAddressCommand request, CancellationToken ct)
    {
        var order = await _orderRepo.GetByIdAsync(request.OrderId, ct);
        if (order is null) return Result.Failure<Guid>("Замовлення не знайдено");
        if (order.BuyerId != request.BuyerId) return Result.Failure<Guid>("Доступ заборонено");

        var existing = await _deliveryRepo.GetByOrderIdAsync(request.OrderId, ct);

        if (existing is null)
        {
            var delivery = Domain.Entities.Delivery.Create(request.OrderId, order.SellerId, request.BuyerId);
            delivery.SetRecipientAddress(
                request.CityRef, request.CityName,
                request.WarehouseRef, request.WarehouseAddress,
                request.RecipientName, request.RecipientPhone);
            await _deliveryRepo.AddAsync(delivery, ct);
            await _deliveryRepo.SaveChangesAsync(ct);
            return Result.Success(delivery.Id);
        }

        existing.SetRecipientAddress(
            request.CityRef, request.CityName,
            request.WarehouseRef, request.WarehouseAddress,
            request.RecipientName, request.RecipientPhone);
        _deliveryRepo.Update(existing);
        await _deliveryRepo.SaveChangesAsync(ct);
        return Result.Success(existing.Id);
    }
}

public record SetSenderAddressCommand(
    Guid OrderId,
    Guid SellerId,
    string CityRef,
    string CityName,
    string WarehouseRef,
    string WarehouseAddress,
    string SenderName,
    string SenderPhone) : IRequest<Result>;

public class SetSenderAddressCommandHandler : IRequestHandler<SetSenderAddressCommand, Result>
{
    private readonly IDeliveryRepository _deliveryRepo;
    private readonly IOrderRepository _orderRepo;

    public SetSenderAddressCommandHandler(IDeliveryRepository deliveryRepo, IOrderRepository orderRepo)
    {
        _deliveryRepo = deliveryRepo;
        _orderRepo = orderRepo;
    }

    public async Task<Result> Handle(SetSenderAddressCommand request, CancellationToken ct)
    {
        var order = await _orderRepo.GetByIdAsync(request.OrderId, ct);
        if (order is null) return Result.Failure("Замовлення не знайдено");
        if (order.SellerId != request.SellerId) return Result.Failure("Доступ заборонено");

        var delivery = await _deliveryRepo.GetByOrderIdAsync(request.OrderId, ct);
        if (delivery is null) return Result.Failure("Спочатку покупець має вказати адресу доставки");

        delivery.SetSenderAddress(
            request.CityRef, request.CityName,
            request.WarehouseRef, request.WarehouseAddress,
            request.SenderName, request.SenderPhone);
        _deliveryRepo.Update(delivery);
        await _deliveryRepo.SaveChangesAsync(ct);
        return Result.Success();
    }
}
