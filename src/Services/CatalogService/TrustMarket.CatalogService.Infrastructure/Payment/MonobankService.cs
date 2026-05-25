using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrustMarket.CatalogService.Application.Abstractions;

namespace TrustMarket.CatalogService.Infrastructure.Payment;

public class MonobankService : IMonobankService
{
    private readonly HttpClient _http;
    private readonly ILogger<MonobankService> _logger;
    private readonly string _token;

    public MonobankService(HttpClient http, IConfiguration configuration, ILogger<MonobankService> logger)
    {
        _http = http;
        _logger = logger;
        _token = configuration["Monobank:Token"] ?? throw new InvalidOperationException("Monobank:Token не налаштовано");
    }

    public async Task<MonobankInvoiceResult> CreateInvoiceAsync(
        decimal amount, string reference, string description,
        string redirectUrl, string webhookUrl,
        CancellationToken ct = default)
    {
        var amountInKopecks = (long)(amount * 100);

        var payload = new
        {
            amount = amountInKopecks,
            ccy = 980,
            merchantPaymInfo = new
            {
                reference,
                destination = description[..Math.Min(description.Length, 100)]
            },
            redirectUrl,
            webHookUrl = webhookUrl
        };

        var request = new HttpRequestMessage(HttpMethod.Post,
            "https://api.monobank.ua/api/merchant/invoice/create");
        request.Headers.Add("X-Token", _token);
        request.Content = JsonContent.Create(payload);

        _logger.LogInformation("Monobank: створення інвойсу на {Amount} коп.", amountInKopecks);

        var response = await _http.SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Monobank API error {Status}: {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"Monobank API error: {body}");
        }

        var result = JsonSerializer.Deserialize<MonobankCreateResponse>(body);
        return new MonobankInvoiceResult(result!.InvoiceId, result.PageUrl);
    }

    private record MonobankCreateResponse(
        [property: JsonPropertyName("invoiceId")] string InvoiceId,
        [property: JsonPropertyName("pageUrl")] string PageUrl);
}
