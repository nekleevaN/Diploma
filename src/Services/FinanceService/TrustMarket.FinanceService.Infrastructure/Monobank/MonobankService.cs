using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrustMarket.FinanceService.Application.Abstractions;

namespace TrustMarket.FinanceService.Infrastructure.Monobank;

public class MonobankService : IMonobankService
{
    private readonly HttpClient _http;
    private readonly ILogger<MonobankService> _logger;
    private readonly string _token;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private const string BaseUrl = "https://api.monobank.ua";

    public MonobankService(HttpClient http, IConfiguration configuration, ILogger<MonobankService> logger)
    {
        _http = http;
        _logger = logger;
        _token = configuration["Monobank:Token"]
            ?? throw new InvalidOperationException("Monobank:Token не налаштовано в appsettings");
    }

    public async Task<MonobankInvoiceResult> CreateHoldInvoiceAsync(
        decimal amount, string reference, string description,
        string redirectUrl, string webhookUrl,
        IReadOnlyList<MonobankSplitRule>? splitRules = null,
        CancellationToken ct = default)
    {
        if (amount <= 0)
            throw new ArgumentException("Сума має бути більшою за 0", nameof(amount));

        var amountInKopecks = (long)(amount * 100);
        var safeDescription = description.Length > 280 ? description[..280] : description;

        var payload = new MonobankCreateInvoiceRequest
        {
            Amount = amountInKopecks,
            Ccy = 980,
            PaymentType = "hold",
            MerchantPaymInfo = new MerchantPaymInfo { Reference = reference, Destination = safeDescription },
            RedirectUrl = redirectUrl,
            WebHookUrl = string.IsNullOrWhiteSpace(webhookUrl) ? null : webhookUrl,
            Validity = 3600,
            SplitRules = splitRules?.Select(r => new MonobankSplitRuleDto
            {
                MerchantId  = r.SubMerchantId,
                Amount      = r.AmountKopecks,
                Description = r.Description
            }).ToList()
        };

        if (splitRules?.Count > 0)
            _logger.LogInformation(
                "Monobank HOLD з split: {Amount} коп, продавець отримає {SellerAmount} коп, ref={Reference}",
                amountInKopecks, splitRules[0].AmountKopecks, reference);
        else
            _logger.LogInformation(
                "Monobank HOLD без split (вся сума до платформи): {Amount} коп, ref={Reference}",
                amountInKopecks, reference);

        var response = await SendAsync<MonobankCreateInvoiceResponse>(
            HttpMethod.Post, "/api/merchant/invoice/create", payload, ct);

        return new MonobankInvoiceResult(response.InvoiceId, response.PageUrl);
    }

    public async Task<bool> FinalizeHoldAsync(string invoiceId, decimal amountUah, CancellationToken ct = default)
    {
        var amountKop = (long)(amountUah * 100);
        _logger.LogInformation("Monobank FinalizeHold: {InvoiceId}, amount={Amount} коп.", invoiceId, amountKop);
        try
        {
            var response = await SendAsync<MonobankFinalizeResponse>(
                HttpMethod.Post, "/api/merchant/invoice/finalize",
                new { invoiceId, amount = amountKop }, ct);
            return response.Status == "success";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка фіналізації {InvoiceId}", invoiceId);
            return false;
        }
    }

    public async Task<bool> CancelInvoiceAsync(string invoiceId, CancellationToken ct = default)
    {
        _logger.LogInformation("Monobank Cancel: {InvoiceId}", invoiceId);
        try
        {
            var response = await SendAsync<MonobankCancelResponse>(
                HttpMethod.Post, "/api/merchant/invoice/cancel", new { invoiceId }, ct);
            return response.Status is "success" or "processing";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка скасування {InvoiceId}", invoiceId);
            return false;
        }
    }

    public async Task<MonobankInvoiceStatus> GetInvoiceStatusAsync(string invoiceId, CancellationToken ct = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{BaseUrl}/api/merchant/invoice/status?invoiceId={invoiceId}");
        request.Headers.Add("X-Token", _token);

        var response = await _http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Monobank status error: {body}");

        return JsonSerializer.Deserialize<MonobankInvoiceStatus>(body, JsonOptions)
            ?? throw new InvalidOperationException("Empty status response");
    }

    private async Task<TResponse> SendAsync<TResponse>(
        HttpMethod method, string path, object payload, CancellationToken ct)
    {
        var request = new HttpRequestMessage(method, $"{BaseUrl}{path}");
        request.Headers.Add("X-Token", _token);
        request.Content = JsonContent.Create(payload, options: JsonOptions);

        var response = await _http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Monobank {Status} on {Path}: {Body}", response.StatusCode, path, body);
            throw new MonobankApiException($"Monobank error ({response.StatusCode}): {body}", (int)response.StatusCode);
        }

        return JsonSerializer.Deserialize<TResponse>(body, JsonOptions)
            ?? throw new InvalidOperationException($"Empty response from {path}");
    }
}

public class MonobankApiException : Exception
{
    public int StatusCode { get; }
    public MonobankApiException(string message, int statusCode) : base(message) => StatusCode = statusCode;
}

internal class MonobankCreateInvoiceRequest
{
    public long Amount { get; set; }
    public int Ccy { get; set; } = 980;
    public string? PaymentType { get; set; }
    public MerchantPaymInfo? MerchantPaymInfo { get; set; }
    public string? RedirectUrl { get; set; }
    public string? WebHookUrl { get; set; }
    public int? Validity { get; set; }

    public List<MonobankSplitRuleDto>? SplitRules { get; set; }
}

internal class MonobankSplitRuleDto
{
    public string MerchantId { get; set; } = null!;
    public long Amount { get; set; }
    public string Description { get; set; } = null!;
}

internal class MerchantPaymInfo
{
    public string? Reference { get; set; }
    public string? Destination { get; set; }
}

internal record MonobankCreateInvoiceResponse(string InvoiceId, string PageUrl);
internal record MonobankFinalizeResponse(string Status);
internal record MonobankCancelResponse(string Status, DateTime? CreatedDate, DateTime? ModifiedDate);
