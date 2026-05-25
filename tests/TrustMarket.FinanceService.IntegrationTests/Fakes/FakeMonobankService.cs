using TrustMarket.FinanceService.Application.Abstractions;

namespace TrustMarket.FinanceService.IntegrationTests.Fakes;

public class FakeMonobankService : IMonobankService
{
    public MonobankInvoiceResult CreateResult { get; set; } =
        new("fake-invoice-id", "https://pay.monobank.ua/fake");
    public bool FinalizeResult { get; set; } = true;
    public bool CancelResult { get; set; } = true;
    public string NextStatus { get; set; } = "hold";

    public Task<MonobankInvoiceResult> CreateHoldInvoiceAsync(
        decimal amount, string reference, string description,
        string redirectUrl, string webhookUrl,
        IReadOnlyList<MonobankSplitRule>? splitRules = null,
        CancellationToken ct = default)
        => Task.FromResult(CreateResult);

    public Task<bool> FinalizeHoldAsync(string invoiceId, decimal amountUah, CancellationToken ct = default)
        => Task.FromResult(FinalizeResult);

    public Task<bool> CancelInvoiceAsync(string invoiceId, CancellationToken ct = default)
        => Task.FromResult(CancelResult);

    public Task<MonobankInvoiceStatus> GetInvoiceStatusAsync(string invoiceId, CancellationToken ct = default)
        => Task.FromResult(new MonobankInvoiceStatus
        {
            InvoiceId = invoiceId,
            Status = NextStatus,
            Amount = 0
        });
}
