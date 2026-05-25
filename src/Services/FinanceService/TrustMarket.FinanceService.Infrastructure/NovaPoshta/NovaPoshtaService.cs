using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrustMarket.FinanceService.Application.Abstractions;

namespace TrustMarket.FinanceService.Infrastructure.NovaPoshta;

public class NovaPoshtaService : INovaPoshtaService
{
    private readonly HttpClient _http;
    private readonly ILogger<NovaPoshtaService> _logger;
    private readonly string _apiKey;

    private const string BaseUrl = "https://api.novaposhta.ua/v2.0/json/";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public NovaPoshtaService(HttpClient http, IConfiguration configuration, ILogger<NovaPoshtaService> logger)
    {
        _http = http;
        _logger = logger;
        _apiKey = configuration["NovaPoshta:ApiKey"]
            ?? throw new InvalidOperationException("NovaPoshta:ApiKey не налаштовано");
    }

    public async Task<List<NPCity>> SearchCitiesAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<NPCity>();

        var response = await SendAsync(new
        {
            modelName = "Address",
            calledMethod = "getCities",
            methodProperties = new { FindByString = query, Limit = "20" }
        }, ct);

        return response.Data
            .Select(d => new NPCity(
                d.GetProperty("Ref").GetString() ?? "",
                d.GetProperty("Description").GetString() ?? "",
                TryGet(d, "AreaDescription"),
                TryGet(d, "RegionsDescription")))
            .ToList();
    }

    public async Task<List<NPWarehouse>> GetWarehousesAsync(string cityRef, int page = 1, string? search = null, CancellationToken ct = default)
    {
        var methodProperties = string.IsNullOrWhiteSpace(search)
            ? (object)new { CityRef = cityRef, Limit = "20", Page = page.ToString() }
            : (object)new { CityRef = cityRef, FindByString = search, Limit = "20", Page = "1" };

        var response = await SendAsync(new
        {
            modelName = "Address",
            calledMethod = "getWarehouses",
            methodProperties
        }, ct);

        return response.Data
            .Select(d => new NPWarehouse(
                d.GetProperty("Ref").GetString() ?? "",
                d.GetProperty("Description").GetString() ?? "",
                TryGet(d, "Number"),
                TryGet(d, "ShortAddress")))
            .ToList();
    }

    public async Task<string> CreateWaybillAsync(CreateWaybillRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("НП: створення ТТН для {Recipient} → {City}",
            request.RecipientName, request.RecipientCityRef);

        var response = await SendAsync(new
        {
            modelName = "InternetDocument",
            calledMethod = "save",
            methodProperties = new
            {
                PayerType = request.PayerType,
                PaymentMethod = request.PaymentMethod,
                DateTime = DateTime.Now.ToString("dd.MM.yyyy"),
                CargoType = "Cargo",
                Weight = request.Weight.ToString("F2"),
                ServiceType = "WarehouseWarehouse",
                SeatsAmount = "1",
                Description = request.Description.Length > 200
                    ? request.Description[..200]
                    : request.Description,
                Cost = ((int)(request.Cost)).ToString(),
                CitySender = request.SenderCityRef,
                Sender = "",
                SenderAddress = request.SenderWarehouseRef,
                ContactSender = "",
                SendersPhone = request.SenderPhone.Replace("+", "").Replace(" ", ""),
                CityRecipient = request.RecipientCityRef,
                RecipientAddress = request.RecipientWarehouseRef,
                RecipientsPhone = request.RecipientPhone.Replace("+", "").Replace(" ", ""),
                RecipientName = request.RecipientName,
                RecipientType = "PrivatePerson",
                NewAddress = "1"
            }
        }, ct);

        var ttn = response.Data.First().GetProperty("IntDocNumber").GetString()
            ?? throw new InvalidOperationException("НП не повернула номер ТТН");

        _logger.LogInformation("НП: ТТН згенеровано: {TTN}", ttn);
        return ttn;
    }

    public async Task<NPTrackingStatus> TrackAsync(string ttn, CancellationToken ct = default)
    {
        var response = await SendAsync(new
        {
            modelName = "TrackingDocument",
            calledMethod = "getStatusDocuments",
            methodProperties = new
            {
                Documents = new[] { new { DocumentNumber = ttn } }
            }
        }, ct);

        var doc = response.Data.First();
        return new NPTrackingStatus(
            StatusCode: TryGet(doc, "StatusCode"),
            StatusDescription: TryGet(doc, "Status"),
            WarehouseRecipientAddress: TryGet(doc, "WarehouseRecipientAddress"),
            ScheduledDeliveryDate: TryParseDate(TryGet(doc, "ScheduledDeliveryDate")),
            ActualDeliveryDate: TryParseDate(TryGet(doc, "ActualDeliveryDate")),
            DocumentCost: TryGet(doc, "DocumentCost"));
    }

    private async Task<NPResponse> SendAsync(object body, CancellationToken ct)
    {
        var payload = JsonSerializer.Serialize(body, JsonOpts);
        var node = System.Text.Json.Nodes.JsonNode.Parse(payload)!;
        node["apiKey"] = _apiKey;
        var payloadWithKey = node.ToJsonString();

        var response = await _http.PostAsync(BaseUrl,
            new StringContent(payloadWithKey, System.Text.Encoding.UTF8, "application/json"), ct);

        var json = await response.Content.ReadAsStringAsync(ct);
        _logger.LogDebug("НП response: {Json}", json[..Math.Min(json.Length, 300)]);

        var result = JsonSerializer.Deserialize<NPResponse>(json, JsonOpts)
            ?? throw new InvalidOperationException("Порожня відповідь від НП");

        if (!result.Success || result.Errors?.Count > 0)
        {
            var err = string.Join(", ", result.Errors ?? new());
            _logger.LogError("НП API помилка: {Errors}", err);
            throw new InvalidOperationException($"Нова Пошта: {err}");
        }

        return result;
    }

    private static string TryGet(JsonElement el, string key)
    {
        try { return el.GetProperty(key).GetString() ?? ""; }
        catch { return ""; }
    }

    private static DateTime? TryParseDate(string? s)
    {
        if (string.IsNullOrEmpty(s)) return null;
        if (DateTime.TryParse(s, out var dt)) return dt;
        return null;
    }

    private class NPResponse
    {
        public bool Success { get; set; }
        public List<JsonElement> Data { get; set; } = new();
        public List<string>? Errors { get; set; }
        public List<string>? Warnings { get; set; }
    }
}
