namespace TrustMarket.FinanceService.Application.Abstractions;

public interface INovaPoshtaService
{
    Task<List<NPCity>> SearchCitiesAsync(string query, CancellationToken ct = default);
    Task<List<NPWarehouse>> GetWarehousesAsync(string cityRef, int page = 1, string? search = null, CancellationToken ct = default);
    Task<string> CreateWaybillAsync(CreateWaybillRequest request, CancellationToken ct = default);
    Task<NPTrackingStatus> TrackAsync(string ttn, CancellationToken ct = default);
}

public record NPCity(string Ref, string Description, string Area, string Region);

public record NPWarehouse(string Ref, string Description, string Number, string ShortAddress);

public record CreateWaybillRequest(
    string SenderCityRef,
    string SenderWarehouseRef,
    string SenderName,
    string SenderPhone,
    string RecipientCityRef,
    string RecipientWarehouseRef,
    string RecipientName,
    string RecipientPhone,
    string Description,
    decimal Cost,
    decimal Weight = 1.0m,
    string PayerType = "Recipient",
    string PaymentMethod = "Cash");

public record NPTrackingStatus(
    string StatusCode,
    string StatusDescription,
    string WarehouseRecipientAddress,
    DateTime? ScheduledDeliveryDate,
    DateTime? ActualDeliveryDate,
    string DocumentCost);
