using FluentAssertions;
using MassTransit;
using NSubstitute;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Application.Offers.Commands;
using TrustMarket.CatalogService.Domain.Entities;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using Xunit;

namespace TrustMarket.CatalogService.UnitTests.Offers;

public class CreateOfferCommandHandlerTests
{
    private readonly IOfferRepository _offerRepo = Substitute.For<IOfferRepository>();
    private readonly IAdvertisementRepository _adRepo = Substitute.For<IAdvertisementRepository>();
    private readonly IPublishEndpoint _publisher = Substitute.For<IPublishEndpoint>();

    private CreateOfferCommandHandler CreateHandler() => new(_offerRepo, _adRepo, _publisher);

    private static Advertisement MakeActiveAd(Guid sellerId) =>
        Advertisement.Create("Телефон", "Опис", 2000m, "Електроніка", sellerId, "Іван", 4.5);

    private static CreateOfferCommand ValidCmd(Guid adId, Guid buyerId) =>
        new(adId, buyerId, "Покупець", 1800m);

    [Fact]
    public async Task Handle_AdNotFound_ReturnsFailure()
    {
        _adRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Advertisement?)null);

        var result = await CreateHandler().Handle(ValidCmd(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Handle_AdNotActive_ReturnsFailure()
    {
        var ad = MakeActiveAd(Guid.NewGuid());
        ad.MarkAsReserved();
        _adRepo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(ValidCmd(ad.Id, Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не активне");
    }

    [Fact]
    public async Task Handle_BuyerIsSeller_ReturnsFailure()
    {
        var sellerId = Guid.NewGuid();
        var ad = MakeActiveAd(sellerId);
        _adRepo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(ValidCmd(ad.Id, sellerId), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("своїм оголошенням");
    }

    [Fact]
    public async Task Handle_PriceZeroOrNegative_ReturnsFailure()
    {
        var ad = MakeActiveAd(Guid.NewGuid());
        _adRepo.GetByIdAsync(ad.Id, default).Returns(ad);
        var cmd = new CreateOfferCommand(ad.Id, Guid.NewGuid(), "Покупець", 0m);

        var result = await CreateHandler().Handle(cmd, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("більше нуля");
    }

    [Fact]
    public async Task Handle_ExistingPendingOffer_ReturnsFailure()
    {
        var ad = MakeActiveAd(Guid.NewGuid());
        _adRepo.GetByIdAsync(ad.Id, default).Returns(ad);
        var buyerId = Guid.NewGuid();
        _offerRepo.GetPendingByBuyerAndAdAsync(buyerId, ad.Id, default)
            .Returns(Offer.Create(ad.Id, buyerId, "Покупець", 1500m));

        var result = await CreateHandler().Handle(ValidCmd(ad.Id, buyerId), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("вже є активна");
    }

    [Fact]
    public async Task Handle_HappyPath_CreatesOfferAndPublishesEvent()
    {
        var sellerId = Guid.NewGuid();
        var buyerId = Guid.NewGuid();
        var ad = MakeActiveAd(sellerId);
        _adRepo.GetByIdAsync(ad.Id, default).Returns(ad);
        _offerRepo.GetPendingByBuyerAndAdAsync(buyerId, ad.Id, default).Returns((Offer?)null);

        var result = await CreateHandler().Handle(ValidCmd(ad.Id, buyerId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.OfferId.Should().NotBeEmpty();
        await _offerRepo.Received(1).AddAsync(Arg.Any<Offer>(), Arg.Any<CancellationToken>());
        await _offerRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(
            Arg.Is<OfferCreatedIntegrationEvent>(e =>
                e.BuyerId == buyerId && e.SellerId == sellerId),
            Arg.Any<CancellationToken>());
    }
}
