using FluentAssertions;
using MassTransit;
using NSubstitute;
using TrustMarket.CatalogService.Application.Abstractions;
using TrustMarket.CatalogService.Application.Advertisements.Commands;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using Xunit;

namespace TrustMarket.CatalogService.UnitTests.Advertisements;

public class CreateAdvertisementCommandHandlerTests
{
    private readonly IAdvertisementRepository _repo = Substitute.For<IAdvertisementRepository>();
    private readonly IPublishEndpoint _publisher = Substitute.For<IPublishEndpoint>();

    private CreateAdvertisementCommandHandler CreateHandler() =>
        new(_repo, _publisher);

    private static CreateAdvertisementCommand ValidCmd(Guid? sellerId = null) =>
        new("Телефон Samsung", "Опис товару", 2000m, "Електроніка",
            sellerId ?? Guid.NewGuid(), "Іван", 4.8);

    [Fact]
    public async Task Handle_HappyPath_ReturnsAdvertisementId()
    {
        var result = await CreateHandler().Handle(ValidCmd(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AdvertisementId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_HappyPath_AddsToRepositoryAndSaves()
    {
        await CreateHandler().Handle(ValidCmd(), default);

        await _repo.Received(1).AddAsync(Arg.Any<TrustMarket.CatalogService.Domain.Entities.Advertisement>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HappyPath_PublishesAdvertisementCreatedEvent()
    {
        var sellerId = Guid.NewGuid();

        await CreateHandler().Handle(ValidCmd(sellerId), default);

        await _publisher.Received(1).Publish(
            Arg.Is<AdvertisementCreatedIntegrationEvent>(e =>
                e.SellerId == sellerId && e.Title == "Телефон Samsung"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithLocation_SetsLocationOnAd()
    {
        var cmd = ValidCmd() with
        {
            Latitude = 50.45,
            Longitude = 30.52,
            LocationAddress = "Київ"
        };

        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithoutLocation_DoesNotSetLocation()
    {
        var cmd = ValidCmd();

        var result = await CreateHandler().Handle(cmd, default);

        result.IsSuccess.Should().BeTrue();
    }
}
