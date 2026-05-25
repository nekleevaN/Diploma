using TrustMarket.FinanceService.Application.Abstractions;

namespace TrustMarket.FinanceService.IntegrationTests.Fakes;

public class FakeCatalogServiceClient : ICatalogServiceClient
{
    public AdReservationResult NextReservationResult { get; set; } = new(
        IsSuccess: true,
        IsConflict: false,
        Error: null,
        Data: new AdReservation(Guid.NewGuid(), "Тестовий товар", 1000m, null));

    public Task<AdReservationResult> ReserveAdvertisementAsync(Guid adId, CancellationToken ct = default)
        => Task.FromResult(NextReservationResult);

    public Task UnreserveAdvertisementAsync(Guid adId, CancellationToken ct = default)
        => Task.CompletedTask;
}
