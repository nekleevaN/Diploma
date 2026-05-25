using FluentAssertions;
using NSubstitute;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Application.Offers.Commands;
using TrustMarket.CatalogService.Domain.Entities;
using Xunit;

namespace TrustMarket.CatalogService.UnitTests.Offers;

public class AcceptCounterOfferCommandHandlerTests
{
    private readonly IOfferRepository _offerRepo = Substitute.For<IOfferRepository>();

    private AcceptCounterOfferCommandHandler CreateHandler() => new(_offerRepo);

    private static Offer MakeCounterOffer(Guid buyerId, decimal counterPrice = 1700m)
    {
        var offer = Offer.Create(Guid.NewGuid(), buyerId, "Покупець", 1500m);
        offer.Counter(counterPrice);
        return offer;
    }

    [Fact]
    public async Task Handle_OfferNotFound_ReturnsFailure()
    {
        _offerRepo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Offer?)null);

        var result = await CreateHandler().Handle(new AcceptCounterOfferCommand(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Handle_WrongBuyer_ReturnsFailure()
    {
        var offer = MakeCounterOffer(Guid.NewGuid());
        _offerRepo.GetByIdAsync(offer.Id, default).Returns(offer);

        var result = await CreateHandler().Handle(new AcceptCounterOfferCommand(offer.Id, Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("заборонено");
    }

    [Fact]
    public async Task Handle_OfferNotCounterOffered_ReturnsFailure()
    {
        var buyerId = Guid.NewGuid();
        var offer = Offer.Create(Guid.NewGuid(), buyerId, "Покупець", 1500m);
        _offerRepo.GetByIdAsync(offer.Id, default).Returns(offer);

        var result = await CreateHandler().Handle(new AcceptCounterOfferCommand(offer.Id, buyerId), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("зустрічною");
    }

    [Fact]
    public async Task Handle_HappyPath_ReturnsAgreedCounterPrice()
    {
        var buyerId = Guid.NewGuid();
        var offer = MakeCounterOffer(buyerId, 1700m);
        _offerRepo.GetByIdAsync(offer.Id, default).Returns(offer);

        var result = await CreateHandler().Handle(new AcceptCounterOfferCommand(offer.Id, buyerId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1700m);
        offer.OfferedPrice.Should().Be(1700m);
        offer.Status.Should().Be(OfferStatus.Accepted);
    }

    [Fact]
    public async Task Handle_HappyPath_SavesChanges()
    {
        var buyerId = Guid.NewGuid();
        var offer = MakeCounterOffer(buyerId);
        _offerRepo.GetByIdAsync(offer.Id, default).Returns(offer);

        await CreateHandler().Handle(new AcceptCounterOfferCommand(offer.Id, buyerId), default);

        _offerRepo.Received(1).Update(offer);
        await _offerRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
