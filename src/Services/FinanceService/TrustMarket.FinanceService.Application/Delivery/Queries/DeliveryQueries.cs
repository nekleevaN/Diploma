using MediatR;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Domain.Entities;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.FinanceService.Application.Delivery.Queries;

public record DeliveryDto(
    Guid DeliveryId,
    Guid OrderId,
    string Status,
    string? RecipientCityName,
    string? RecipientWarehouseAddress,
    string? RecipientName,
    string? RecipientPhone,
    string? SenderCityName,
    string? SenderWarehouseAddress,
    string? SenderName,
    string? TTN,
    string? TrackingStatus,
    string? TrackingStatusDescription,
    DateTime? EstimatedDeliveryDate);

public record GetDeliveryByOrderQuery(Guid OrderId, Guid RequesterId) : IRequest<Result<DeliveryDto>>;

public record TrackDeliveryQuery(Guid OrderId, Guid RequesterId) : IRequest<Result<DeliveryDto>>;

public record SearchCitiesQuery(string Query) : IRequest<List<NPCity>>;

public record GetWarehousesQuery(string CityRef, int Page = 1, string? Search = null) : IRequest<List<NPWarehouse>>;

public class GetDeliveryByOrderQueryHandler : IRequestHandler<GetDeliveryByOrderQuery, Result<DeliveryDto>>
{
    private readonly IDeliveryRepository _repo;
    private readonly IOrderRepository _orderRepo;

    public GetDeliveryByOrderQueryHandler(IDeliveryRepository repo, IOrderRepository orderRepo)
    {
        _repo = repo;
        _orderRepo = orderRepo;
    }

    public async Task<Result<DeliveryDto>> Handle(GetDeliveryByOrderQuery request, CancellationToken ct)
    {
        var order = await _orderRepo.GetByIdAsync(request.OrderId, ct);
        if (order is null) return Result.Failure<DeliveryDto>("Замовлення не знайдено");
        if (order.BuyerId != request.RequesterId && order.SellerId != request.RequesterId)
            return Result.Failure<DeliveryDto>("Доступ заборонено");

        var delivery = await _repo.GetByOrderIdAsync(request.OrderId, ct);
        if (delivery is null) return Result.Failure<DeliveryDto>("Доставку ще не оформлено");

        return Result.Success(DeliveryMapper.ToDto(delivery));
    }
}

public class TrackDeliveryQueryHandler : IRequestHandler<TrackDeliveryQuery, Result<DeliveryDto>>
{
    private readonly IDeliveryRepository _repo;
    private readonly IOrderRepository _orderRepo;
    private readonly INovaPoshtaService _np;

    public TrackDeliveryQueryHandler(IDeliveryRepository repo, IOrderRepository orderRepo, INovaPoshtaService np)
    {
        _repo = repo;
        _orderRepo = orderRepo;
        _np = np;
    }

    public async Task<Result<DeliveryDto>> Handle(TrackDeliveryQuery request, CancellationToken ct)
    {
        var order = await _orderRepo.GetByIdAsync(request.OrderId, ct);
        if (order is null) return Result.Failure<DeliveryDto>("Замовлення не знайдено");
        if (order.BuyerId != request.RequesterId && order.SellerId != request.RequesterId)
            return Result.Failure<DeliveryDto>("Доступ заборонено");

        var delivery = await _repo.GetByOrderIdAsync(request.OrderId, ct);
        if (delivery is null) return Result.Failure<DeliveryDto>("Доставку ще не оформлено");

        if (!string.IsNullOrEmpty(delivery.TTN))
        {
            try
            {
                var status = await _np.TrackAsync(delivery.TTN, ct);
                delivery.UpdateTracking(status.StatusCode, status.StatusDescription, status.ScheduledDeliveryDate);
                _repo.Update(delivery);
                await _repo.SaveChangesAsync(ct);
            }
            catch {  }
        }

        return Result.Success(DeliveryMapper.ToDto(delivery));
    }
}

public class SearchCitiesQueryHandler : IRequestHandler<SearchCitiesQuery, List<NPCity>>
{
    private readonly INovaPoshtaService _np;
    public SearchCitiesQueryHandler(INovaPoshtaService np) => _np = np;
    public Task<List<NPCity>> Handle(SearchCitiesQuery request, CancellationToken ct)
        => _np.SearchCitiesAsync(request.Query, ct);
}

public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, List<NPWarehouse>>
{
    private readonly INovaPoshtaService _np;
    public GetWarehousesQueryHandler(INovaPoshtaService np) => _np = np;
    public Task<List<NPWarehouse>> Handle(GetWarehousesQuery request, CancellationToken ct)
        => _np.GetWarehousesAsync(request.CityRef, request.Page, request.Search, ct);
}

internal static class DeliveryMapper
{
    public static DeliveryDto ToDto(TrustMarket.FinanceService.Domain.Entities.Delivery d) => new(
    d.Id, d.OrderId, d.Status.ToString(),
    d.RecipientCityName, d.RecipientWarehouseAddress, d.RecipientName, d.RecipientPhone,
    d.SenderCityName, d.SenderWarehouseAddress, d.SenderName,
    d.TTN, d.TrackingStatus, d.TrackingStatusDescription, d.EstimatedDeliveryDate);
}
