using MediatR;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.Shared.Common.Results;

namespace TrustMarket.CatalogService.Application.Payments.Commands;

public record CreatePaymentCommand(
    Guid AdvertisementId,
    Guid BuyerId,
    string BaseUrl) : IRequest<Result<CreatePaymentResponse>>;

public record CreatePaymentResponse(string PageUrl, string InvoiceId);

public class CreatePaymentCommandHandler
    : IRequestHandler<CreatePaymentCommand, Result<CreatePaymentResponse>>
{
    private readonly IAdvertisementRepository _repository;
    private readonly IMonobankService _monobank;

    public CreatePaymentCommandHandler(IAdvertisementRepository repository, IMonobankService monobank)
    {
        _repository = repository;
        _monobank = monobank;
    }

    public async Task<Result<CreatePaymentResponse>> Handle(
        CreatePaymentCommand request, CancellationToken ct)
    {
        var ad = await _repository.GetByIdAsync(request.AdvertisementId, ct);
        if (ad is null)
            return Result.Failure<CreatePaymentResponse>("Оголошення не знайдено");

        if (ad.Status != Domain.Entities.AdvertisementStatus.Active)
            return Result.Failure<CreatePaymentResponse>("Оголошення вже продано або знято");

        var redirectUrl = $"{request.BaseUrl}/payment/success?adId={ad.Id}";
        var webhookUrl = $"{request.BaseUrl}/api/payment/webhook";

        var result = await _monobank.CreateInvoiceAsync(
            amount: ad.Price,
            reference: $"AD-{ad.Id}",
            description: ad.Title,
            redirectUrl: redirectUrl,
            webhookUrl: webhookUrl,
            ct: ct);

        return Result.Success(new CreatePaymentResponse(result.PageUrl, result.InvoiceId));
    }
}
