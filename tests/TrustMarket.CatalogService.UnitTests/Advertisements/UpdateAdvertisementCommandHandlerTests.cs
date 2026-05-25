using FluentAssertions;
using NSubstitute;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Application.Advertisements.Commands;
using TrustMarket.CatalogService.Domain.Entities;
using Xunit;

namespace TrustMarket.CatalogService.UnitTests.Advertisements;

public class UpdateAdvertisementCommandHandlerTests
{
    private readonly IAdvertisementRepository _repo = Substitute.For<IAdvertisementRepository>();

    private UpdateAdvertisementCommandHandler CreateHandler() => new(_repo);

    private static Advertisement MakeAd(Guid sellerId) =>
        Advertisement.Create("Old", "Old desc", 100m, "Cat", sellerId, "Іван", 4.0);

    private static UpdateAdvertisementCommand ValidCmd(Guid adId, Guid sellerId) =>
        new(adId, sellerId, "New Title", "New desc", 500m, "Електроніка");

    [Fact]
    public async Task Handle_AdNotFound_ReturnsFailure()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Advertisement?)null);

        var result = await CreateHandler().Handle(ValidCmd(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Handle_WrongSeller_ReturnsFailure()
    {
        var ad = MakeAd(Guid.NewGuid());
        _repo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(ValidCmd(ad.Id, Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("заборонено");
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesDetailsAndSaves()
    {
        var sellerId = Guid.NewGuid();
        var ad = MakeAd(sellerId);
        _repo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(ValidCmd(ad.Id, sellerId), default);

        result.IsSuccess.Should().BeTrue();
        ad.Title.Should().Be("New Title");
        ad.Price.Should().Be(500m);
        _repo.Received(1).Update(ad);
        await _repo.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_WithNewLocation_SetsLocation()
    {
        var sellerId = Guid.NewGuid();
        var ad = MakeAd(sellerId);
        _repo.GetByIdAsync(ad.Id, default).Returns(ad);
        var cmd = ValidCmd(ad.Id, sellerId) with { Latitude = 50.45, Longitude = 30.52, LocationAddress = "Київ" };

        await CreateHandler().Handle(cmd, default);

        ad.HasLocation.Should().BeTrue();
        ad.LocationAddress.Should().Be("Київ");
    }

    [Fact]
    public async Task Handle_ClearLocation_RemovesLocation()
    {
        var sellerId = Guid.NewGuid();
        var ad = MakeAd(sellerId);
        ad.SetLocation(50.45, 30.52, "Київ");
        _repo.GetByIdAsync(ad.Id, default).Returns(ad);
        var cmd = ValidCmd(ad.Id, sellerId) with { ClearLocation = true };

        await CreateHandler().Handle(cmd, default);

        ad.HasLocation.Should().BeFalse();
    }
}
