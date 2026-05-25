using FluentAssertions;
using TrustMarket.CatalogService.Domain.Entities;
using Xunit;

namespace TrustMarket.CatalogService.UnitTests.Domain;

public class OfferTests
{
    private static Offer Make() =>
        Offer.Create(Guid.NewGuid(), Guid.NewGuid(), "Іван", 1500m);

    [Fact]
    public void Create_SetsPendingStatusAndFields()
    {
        var offer = Make();

        offer.Status.Should().Be(OfferStatus.Pending);
        offer.OfferedPrice.Should().Be(1500m);
        offer.CounterPrice.Should().BeNull();
    }

    [Fact]
    public void Accept_SetsAcceptedStatus()
    {
        var offer = Make();

        offer.Accept();

        offer.Status.Should().Be(OfferStatus.Accepted);
    }

    [Fact]
    public void Reject_SetsRejectedStatusAndStoresNote()
    {
        var offer = Make();

        offer.Reject("Ціна занадто низька");

        offer.Status.Should().Be(OfferStatus.Rejected);
        offer.SellerNote.Should().Be("Ціна занадто низька");
    }

    [Fact]
    public void Counter_SetsCounterOfferedStatusWithPrice()
    {
        var offer = Make();

        offer.Counter(1700m, "Пропоную 1700");

        offer.Status.Should().Be(OfferStatus.CounterOffered);
        offer.CounterPrice.Should().Be(1700m);
        offer.SellerNote.Should().Be("Пропоную 1700");
    }

    [Fact]
    public void AcceptCounter_UpdatesOfferedPriceAndClearsCounter()
    {
        var offer = Make();
        offer.Counter(1700m);

        offer.AcceptCounter();

        offer.Status.Should().Be(OfferStatus.Accepted);
        offer.OfferedPrice.Should().Be(1700m);
        offer.CounterPrice.Should().BeNull();
    }

    [Fact]
    public void AcceptCounter_WhenNotCounterOffered_Throws()
    {
        var offer = Make();

        var act = () => offer.AcceptCounter();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*CounterOffered*");
    }

    [Fact]
    public void AcceptCounter_WhenAlreadyAccepted_Throws()
    {
        var offer = Make();
        offer.Accept();

        var act = () => offer.AcceptCounter();

        act.Should().Throw<InvalidOperationException>();
    }
}
