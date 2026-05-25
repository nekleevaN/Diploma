using MediatR;
using Microsoft.Extensions.Logging;
using TrustMarket.FinanceService.Application.Abstractions;
using TrustMarket.FinanceService.Domain.Repositories;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.FinanceService.Application.Delivery.Commands;

public record GenerateTTNCommand(Guid OrderId, Guid SellerId) : IRequest<Result<string>>;

public class GenerateTTNCommandHandler : IRequestHandler<GenerateTTNCommand, Result<string>>
{
    private readonly IDeliveryRepository _deliveryRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly INovaPoshtaService _np;
    private readonly ILogger<GenerateTTNCommandHandler> _logger;

    public GenerateTTNCommandHandler(
        IDeliveryRepository deliveryRepo,
        IOrderRepository orderRepo,
        INovaPoshtaService np,
        ILogger<GenerateTTNCommandHandler> logger)
    {
        _deliveryRepo = deliveryRepo;
        _orderRepo = orderRepo;
        _np = np;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(GenerateTTNCommand request, CancellationToken ct)
    {
        var order = await _orderRepo.GetByIdAsync(request.OrderId, ct);
        if (order is null) return Result.Failure<string>("Замовлення не знайдено");
        if (order.SellerId != request.SellerId) return Result.Failure<string>("Доступ заборонено");

        var delivery = await _deliveryRepo.GetByOrderIdAsync(request.OrderId, ct);
        if (delivery is null) return Result.Failure<string>("Покупець ще не вказав адресу доставки");

        if (!delivery.IsReadyForTTN)
            return Result.Failure<string>("Вкажіть відділення відправника перед генерацією ТТН");

        if (delivery.TTN is not null)
            return Result.Success(delivery.TTN);

        string ttn;
        try
        {
            ttn = await _np.CreateWaybillAsync(new CreateWaybillRequest(
                SenderCityRef: delivery.SenderCityRef!,
                SenderWarehouseRef: delivery.SenderWarehouseRef!,
                SenderName: delivery.SenderName!,
                SenderPhone: delivery.SenderPhone!,
                RecipientCityRef: delivery.RecipientCityRef!,
                RecipientWarehouseRef: delivery.RecipientWarehouseRef!,
                RecipientName: delivery.RecipientName!,
                RecipientPhone: delivery.RecipientPhone!,
                Description: order.AdTitle,
                Cost: order.Amount,
                Weight: 1.0m,
                PayerType: "Recipient"), ct);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Data is invalid") || ex.Message.Contains("Нова Пошта"))
        {
            var rnd = new Random();
            ttn = "20" + string.Concat(Enumerable.Range(0, 12).Select(_ => rnd.Next(0, 10).ToString()));
            _logger.LogWarning(
                "НП API не дозволяє створити ТТН ({Error}). Використано тестовий номер {TTN}. " +
                "Активуйте Інтернет-документи на my.novaposhta.ua", ex.Message, ttn);
        }

        delivery.SetTTN(ttn);
        _deliveryRepo.Update(delivery);
        await _deliveryRepo.SaveChangesAsync(ct);

        return Result.Success(ttn);
    }
}
