using FluentAssertions;
using NSubstitute;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Application.Advertisements.Commands;
using TrustMarket.CatalogService.Domain.Entities;
using Xunit;

namespace TrustMarket.CatalogService.UnitTests.Advertisements;

public class DeleteAdvertisementCommandHandlerTests
{
    private readonly IAdvertisementRepository _repo = Substitute.For<IAdvertisementRepository>();

    private DeleteAdvertisementCommandHandler CreateHandler() => new(_repo);

    private static Advertisement MakeAd(Guid sellerId) =>
        Advertisement.Create("Товар", "Опис", 500m, "Одяг", sellerId, "Іван", 4.5);

    [Fact]
    public async Task Handle_AdNotFound_ReturnsFailure()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Advertisement?)null);

        var result = await CreateHandler().Handle(new DeleteAdvertisementCommand(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Handle_WrongSeller_ReturnsFailure()
    {
        var ad = MakeAd(Guid.NewGuid());
        _repo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(new DeleteAdvertisementCommand(ad.Id, Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("заборонено");
    }

    [Fact]
    public async Task Handle_ValidRequest_MarksAsRemovedAndSaves()
    {
        var sellerId = Guid.NewGuid();
        var ad = MakeAd(sellerId);
        _repo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(new DeleteAdvertisementCommand(ad.Id, sellerId), default);

        result.IsSuccess.Should().BeTrue();
        ad.Status.Should().Be(AdvertisementStatus.Removed);
        _repo.Received(1).Update(ad);
        await _repo.Received(1).SaveChangesAsync(default);
    }
}
