using MediatR;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.FinanceService.Application.Orders.Queries;

public record OrderDto(
    Guid OrderId,
    Guid AdvertisementId,
    Guid BuyerId,
    Guid SellerId,
    string AdTitle,
    decimal Amount,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record GetMyOrdersAsBuyerQuery(Guid BuyerId) : IRequest<Result<List<OrderDto>>>;
public record GetMyOrdersAsSellerQuery(Guid SellerId) : IRequest<Result<List<OrderDto>>>;
public record GetOrderByIdQuery(Guid OrderId, Guid RequesterId) : IRequest<Result<OrderDto>>;

public class GetMyOrdersAsBuyerQueryHandler : IRequestHandler<GetMyOrdersAsBuyerQuery, Result<List<OrderDto>>>
{
    private readonly IOrderRepository _repo;
    public GetMyOrdersAsBuyerQueryHandler(IOrderRepository repo) => _repo = repo;
    public async Task<Result<List<OrderDto>>> Handle(GetMyOrdersAsBuyerQuery request, CancellationToken ct)
    {
        var orders = await _repo.GetByBuyerAsync(request.BuyerId, ct);
        return Result.Success(orders.Select(OrderMapper.ToDto).ToList());
    }
}

public class GetMyOrdersAsSellerQueryHandler : IRequestHandler<GetMyOrdersAsSellerQuery, Result<List<OrderDto>>>
{
    private readonly IOrderRepository _repo;
    public GetMyOrdersAsSellerQueryHandler(IOrderRepository repo) => _repo = repo;
    public async Task<Result<List<OrderDto>>> Handle(GetMyOrdersAsSellerQuery request, CancellationToken ct)
    {
        var orders = await _repo.GetBySellerAsync(request.SellerId, ct);
        return Result.Success(orders.Select(OrderMapper.ToDto).ToList());
    }
}

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    private readonly IOrderRepository _repo;
    public GetOrderByIdQueryHandler(IOrderRepository repo) => _repo = repo;
    public async Task<Result<OrderDto>> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(request.OrderId, ct);
        if (order is null) return Result.Failure<OrderDto>("Замовлення не знайдено");
        if (order.BuyerId != request.RequesterId && order.SellerId != request.RequesterId)
            return Result.Failure<OrderDto>("Доступ заборонено");
        return Result.Success(OrderMapper.ToDto(order));
    }
}

internal static class OrderMapper
{
    public static OrderDto ToDto(TrustMarket.FinanceService.Domain.Entities.Order o) =>
        new(o.Id, o.AdvertisementId, o.BuyerId, o.SellerId, o.AdTitle, o.Amount, o.Status.ToString(), o.CreatedAt, o.UpdatedAt);
}
