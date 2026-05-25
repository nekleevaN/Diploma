namespace TrustMarket.FinanceService.Application.Abstractions;

public interface IMonobankService
{
    Task<MonobankInvoiceResult> CreateHoldInvoiceAsync(
        decimal amount, string reference, string description,
        string redirectUrl, string webhookUrl,
        IReadOnlyList<MonobankSplitRule>? splitRules = null,
        CancellationToken ct = default);

    Task<bool> FinalizeHoldAsync(string invoiceId, decimal amountUah, CancellationToken ct = default);
    Task<bool> CancelInvoiceAsync(string invoiceId, CancellationToken ct = default);
    Task<MonobankInvoiceStatus> GetInvoiceStatusAsync(string invoiceId, CancellationToken ct = default);
}

public record MonobankInvoiceResult(string InvoiceId, string PageUrl);

public record MonobankSplitRule(
    string SubMerchantId,
    long AmountKopecks,
    string Description);

public class MonobankInvoiceStatus
{
    public string InvoiceId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public long Amount { get; set; }
    public int Ccy { get; set; }
    public long? FinalAmount { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string? Reference { get; set; }
    public string? Destination { get; set; }
    public string? ErrCode { get; set; }
    public string? FailureReason { get; set; }
}
