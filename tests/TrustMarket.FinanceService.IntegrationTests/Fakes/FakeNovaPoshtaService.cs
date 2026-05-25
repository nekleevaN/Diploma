using TrustMarket.FinanceService.Application.Abstractions;

namespace TrustMarket.FinanceService.IntegrationTests.Fakes;

public class FakeNovaPoshtaService : INovaPoshtaService
{
    public string NextTtn { get; set; } = "59000123456789";

    public Task<List<NPCity>> SearchCitiesAsync(string query, CancellationToken ct = default)
        => Task.FromResult(new List<NPCity>
        {
            new("city-ref-001", "Київ", "Київська", "м.")
        });

    public Task<List<NPWarehouse>> GetWarehousesAsync(string cityRef, int page = 1, string? search = null, CancellationToken ct = default)
        => Task.FromResult(new List<NPWarehouse>
        {
            new("wh-ref-001", "Відділення №1", "1", "вул. Тестова, 1")
        });

    public Task<string> CreateWaybillAsync(CreateWaybillRequest request, CancellationToken ct = default)
        => Task.FromResult(NextTtn);

    public Task<NPTrackingStatus> TrackAsync(string ttn, CancellationToken ct = default)
        => Task.FromResult(new NPTrackingStatus("5", "Відправлення прибуло", "м. Київ", null, null, "0"));
}
