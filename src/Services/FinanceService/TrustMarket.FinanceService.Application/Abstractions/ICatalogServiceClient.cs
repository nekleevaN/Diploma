namespace TrustMarket.FinanceService.Application.Abstractions;

public record AdReservation(
    Guid SellerId,
    string Title,
    decimal Price,
    string? SellerSubMerchantId = null);

public record AdReservationResult(
    bool IsSuccess,
    bool IsConflict,
    string? Error,
    AdReservation? Data);

public interface ICatalogServiceClient
{
    Task<AdReservationResult> ReserveAdvertisementAsync(Guid adId, CancellationToken ct = default);
    Task UnreserveAdvertisementAsync(Guid adId, CancellationToken ct = default);
}
