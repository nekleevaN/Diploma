using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TrustMarket.FinanceService.Application.Abstractions;

namespace TrustMarket.FinanceService.Infrastructure.CatalogService;

public class CatalogServiceClient : ICatalogServiceClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _ctx;
    private readonly ILogger<CatalogServiceClient> _logger;

    public CatalogServiceClient(
        HttpClient http,
        IHttpContextAccessor ctx,
        ILogger<CatalogServiceClient> logger)
    {
        _http = http;
        _ctx = ctx;
        _logger = logger;
    }

    public async Task<AdReservationResult> ReserveAdvertisementAsync(Guid adId, CancellationToken ct = default)
    {
        ForwardAuth();

        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsync($"api/ads/{adId}/reserve", null, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CatalogService недоступний при резервуванні {AdId}", adId);
            return new AdReservationResult(false, false, "Сервіс тимчасово недоступний", null);
        }

        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<AdReservationData>(
                cancellationToken: ct);
            return new AdReservationResult(
                true, false, null,
                new AdReservation(data!.SellerId, data.Title, data.Price,
                    data.SellerSubMerchantId));
        }

        if ((int)response.StatusCode == 409)
            return new AdReservationResult(false, true, "Товар вже придбав інший покупець", null);

        var err = await response.Content.ReadAsStringAsync(ct);
        _logger.LogWarning("Резервування {AdId} відхилено: {Error}", adId, err);
        return new AdReservationResult(false, false, "Не вдалося зарезервувати товар", null);
    }

    public async Task UnreserveAdvertisementAsync(Guid adId, CancellationToken ct = default)
    {
        try
        {
            ForwardAuth();
            await _http.PostAsync($"api/ads/{adId}/unreserve", null, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не вдалося відкотити резервування {AdId}", adId);
        }
    }

    private void ForwardAuth()
    {
        var token = _ctx.HttpContext?.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);
    }

    private record AdReservationData(
        Guid SellerId, string Title, decimal Price,
        string? SellerSubMerchantId = null);
}
