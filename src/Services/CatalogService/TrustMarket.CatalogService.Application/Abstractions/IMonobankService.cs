namespace TrustMarket.CatalogService.Application.Abstractions;

public interface IMonobankService
{
    Task<MonobankInvoiceResult> CreateInvoiceAsync(
        decimal amount, string reference, string description,
        string redirectUrl, string webhookUrl,
        CancellationToken ct = default);
}

public record MonobankInvoiceResult(string InvoiceId, string PageUrl);
