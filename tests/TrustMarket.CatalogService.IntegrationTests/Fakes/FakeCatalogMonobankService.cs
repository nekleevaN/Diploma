using TrustMarket.CatalogService.Application.Abstractions;

namespace TrustMarket.CatalogService.IntegrationTests.Fakes;

public class FakeCatalogMonobankService : IMonobankService
{
    public MonobankInvoiceResult NextResult { get; set; } =
        new("fake-invoice-id", "https://pay.monobank.ua/fake");

    public Task<MonobankInvoiceResult> CreateInvoiceAsync(
        decimal amount, string reference, string description,
        string redirectUrl, string webhookUrl,
        CancellationToken ct = default)
        => Task.FromResult(NextResult);
}
