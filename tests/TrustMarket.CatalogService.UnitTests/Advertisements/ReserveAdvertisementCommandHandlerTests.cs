using FluentAssertions;
using NSubstitute;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Application.Advertisements.Commands;
using TrustMarket.CatalogService.Domain.Entities;
using Xunit;

namespace TrustMarket.CatalogService.UnitTests.Advertisements;

public class ReserveAdvertisementCommandHandlerTests
{
    private readonly IAdvertisementRepository _repo = Substitute.For<IAdvertisementRepository>();

    private ReserveAdvertisementCommandHandler CreateHandler() => new(_repo);
    private UnreserveAdvertisementCommandHandler CreateUnreserveHandler() => new(_repo);

    private static Advertisement MakeAd(Guid sellerId, string? subMerchantId = "sub_123") =>
        Advertisement.Create("Товар", "Опис", 1500m, "Електроніка", sellerId, "Іван", 4.8);

    [Fact]
    public async Task Reserve_AdNotFound_ReturnsFailure()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Advertisement?)null);

        var result = await CreateHandler().Handle(new ReserveAdvertisementCommand(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Reserve_BuyerIsSeller_ReturnsFailure()
    {
        var sellerId = Guid.NewGuid();
        var ad = MakeAd(sellerId);
        _repo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(new ReserveAdvertisementCommand(ad.Id, sellerId), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("власне");
    }

    [Fact]
    public async Task Reserve_AlreadyReserved_ReturnsConflictFailure()
    {
        var sellerId = Guid.NewGuid();
        var ad = MakeAd(sellerId);
        ad.MarkAsReserved();
        _repo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(new ReserveAdvertisementCommand(ad.Id, Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().StartWith("CONFLICT:");
    }

    [Fact]
    public async Task Reserve_HappyPath_MarksAsReservedAndReturnsInfo()
    {
        var sellerId = Guid.NewGuid();
        var ad = MakeAd(sellerId);
        _repo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateHandler().Handle(new ReserveAdvertisementCommand(ad.Id, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SellerId.Should().Be(sellerId);
        result.Value.Price.Should().Be(1500m);
        ad.Status.Should().Be(AdvertisementStatus.Reserved);
        _repo.Received(1).Update(ad);
        await _repo.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Unreserve_AdNotFound_ReturnsFailure()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).Returns((Advertisement?)null);

        var result = await CreateUnreserveHandler().Handle(new UnreserveAdvertisementCommand(Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Unreserve_WhenReserved_RestoresToActiveAndSaves()
    {
        var ad = MakeAd(Guid.NewGuid());
        ad.MarkAsReserved();
        _repo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateUnreserveHandler().Handle(new UnreserveAdvertisementCommand(ad.Id), default);

        result.IsSuccess.Should().BeTrue();
        ad.Status.Should().Be(AdvertisementStatus.Active);
        _repo.Received(1).Update(ad);
        await _repo.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Unreserve_WhenAlreadyActive_SucceedsWithoutUpdate()
    {
        var ad = MakeAd(Guid.NewGuid());
        _repo.GetByIdAsync(ad.Id, default).Returns(ad);

        var result = await CreateUnreserveHandler().Handle(new UnreserveAdvertisementCommand(ad.Id), default);

        result.IsSuccess.Should().BeTrue();
        _repo.DidNotReceive().Update(Arg.Any<Advertisement>());
    }
}
