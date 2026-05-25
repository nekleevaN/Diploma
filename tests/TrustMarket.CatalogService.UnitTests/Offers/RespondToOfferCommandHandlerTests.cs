using FluentAssertions;
using MassTransit;
using NSubstitute;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Application.Offers.Commands;
using TrustMarket.CatalogService.Domain.Entities;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using Xunit;

namespace TrustMarket.CatalogService.UnitTests.Offers;

public class RespondToOfferCommandHandlerTests
{
    private readonly IOfferRepository _offerRepo = Substitute.For<IOfferRepository>();
    private readonly IAdvertisementRepository _adRepo = Substitute.For<IAdvertisementRepository>();
    private readonly IPublishEndpoint _publisher = Substitute.For<IPublishEndpoint>();

    private RespondToOfferCommandHandler CreateHandler() => new(_offerRepo, _adRepo, _publisher);

    private static (Advertisement ad, Offer offer) MakePair(Guid sellerId)
    {
        var ad = Advertisement.Create("Товар", "Опис", 2000m, "Cat", sellerId, "Продавець", 4.5);
        var offer = Offer.Create(ad.Id, Guid.NewGuid(), "Покупець", 1800m);
        return (ad, offer);
    }

    [Fact]
    public async Task Handle_OfferNotFound_ReturnsFailure()
    {
        _offerRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Offer?)null);

        var result = await CreateHandler().Handle(
            new RespondToOfferCommand(Guid.NewGuid(), Guid.NewGuid(), "accept", null, null), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Handle_WrongSeller_ReturnsFailure()
    {
        var (ad, offer) = MakePair(Guid.NewGuid());
        _offerRepo.GetByIdAsync(offer.Id, default).Returns(offer);
        _adRepo.GetByIdAsync(offer.AdvertisementId, default).Returns(ad);

        var result = await CreateHandler().Handle(
            new RespondToOfferCommand(offer.Id, Guid.NewGuid(), "accept", null, null), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("заборонено");
    }

    [Fact]
    public async Task Handle_OfferAlreadyAnswered_ReturnsFailure()
    {
        var sellerId = Guid.NewGuid();
        var (ad, offer) = MakePair(sellerId);
        offer.Accept();
        _offerRepo.GetByIdAsync(offer.Id, default).Returns(offer);
        _adRepo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(
            new RespondToOfferCommand(offer.Id, sellerId, "accept", null, null), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("вже відповіли");
    }

    [Fact]
    public async Task Handle_Accept_SetsAcceptedAndPublishesEvent()
    {
        var sellerId = Guid.NewGuid();
        var (ad, offer) = MakePair(sellerId);
        _offerRepo.GetByIdAsync(offer.Id, default).Returns(offer);
        _adRepo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(
            new RespondToOfferCommand(offer.Id, sellerId, "accept", null, null), default);

        result.IsSuccess.Should().BeTrue();
        offer.Status.Should().Be(OfferStatus.Accepted);
        await _publisher.Received(1).Publish(Arg.Any<OfferRespondedIntegrationEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Reject_SetsRejectedWithNote()
    {
        var sellerId = Guid.NewGuid();
        var (ad, offer) = MakePair(sellerId);
        _offerRepo.GetByIdAsync(offer.Id, default).Returns(offer);
        _adRepo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(
            new RespondToOfferCommand(offer.Id, sellerId, "reject", null, "Не домовились"), default);

        result.IsSuccess.Should().BeTrue();
        offer.Status.Should().Be(OfferStatus.Rejected);
        offer.SellerNote.Should().Be("Не домовились");
    }

    [Fact]
    public async Task Handle_Counter_SetsCounterOfferedWithPrice()
    {
        var sellerId = Guid.NewGuid();
        var (ad, offer) = MakePair(sellerId);
        _offerRepo.GetByIdAsync(offer.Id, default).Returns(offer);
        _adRepo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(
            new RespondToOfferCommand(offer.Id, sellerId, "counter", 1900m, "Пропоную 1900"), default);

        result.IsSuccess.Should().BeTrue();
        offer.Status.Should().Be(OfferStatus.CounterOffered);
        offer.CounterPrice.Should().Be(1900m);
    }

    [Fact]
    public async Task Handle_CounterWithoutPrice_ReturnsFailure()
    {
        var sellerId = Guid.NewGuid();
        var (ad, offer) = MakePair(sellerId);
        _offerRepo.GetByIdAsync(offer.Id, default).Returns(offer);
        _adRepo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(
            new RespondToOfferCommand(offer.Id, sellerId, "counter", null, null), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("зустрічну ціну");
    }

    [Fact]
    public async Task Handle_UnknownAction_ReturnsFailure()
    {
        var sellerId = Guid.NewGuid();
        var (ad, offer) = MakePair(sellerId);
        _offerRepo.GetByIdAsync(offer.Id, default).Returns(offer);
        _adRepo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(
            new RespondToOfferCommand(offer.Id, sellerId, "invalidaction", null, null), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Невідома");
    }

    [Fact]
    public async Task Handle_ValidResponse_SavesChanges()
    {
        var sellerId = Guid.NewGuid();
        var (ad, offer) = MakePair(sellerId);
        _offerRepo.GetByIdAsync(offer.Id, default).Returns(offer);
        _adRepo.GetByIdAsync(ad.Id, default).Returns(ad);

        await CreateHandler().Handle(
            new RespondToOfferCommand(offer.Id, sellerId, "accept", null, null), default);

        _offerRepo.Received(1).Update(offer);
        await _offerRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
