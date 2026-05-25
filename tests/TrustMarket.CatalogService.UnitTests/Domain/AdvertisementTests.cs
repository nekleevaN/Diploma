using FluentAssertions;
using TrustMarket.CatalogService.Domain.Entities;
using Xunit;

namespace TrustMarket.CatalogService.UnitTests.Domain;

public class AdvertisementTests
{
    private static Advertisement Make(Guid? sellerId = null) =>
        Advertisement.Create(
            "Телефон Samsung", "Опис", 2000m, "Електроніка",
            sellerId ?? Guid.NewGuid(), "Іван", 4.8);

    [Fact]
    public void Create_SetsActiveStatusAndCoreFields()
    {
        var ad = Make();

        ad.Status.Should().Be(AdvertisementStatus.Active);
        ad.Title.Should().Be("Телефон Samsung");
        ad.Price.Should().Be(2000m);
        ad.SellerSubMerchantId.Should().BeNull();
    }

    [Fact]
    public void Remove_SetsRemovedStatus()
    {
        var ad = Make();

        ad.Remove();

        ad.Status.Should().Be(AdvertisementStatus.Removed);
    }

    [Fact]
    public void MarkAsReserved_SetsReservedStatus()
    {
        var ad = Make();

        ad.MarkAsReserved();

        ad.Status.Should().Be(AdvertisementStatus.Reserved);
    }

    [Fact]
    public void MarkAsSold_SetsSoldStatus()
    {
        var ad = Make();

        ad.MarkAsSold();

        ad.Status.Should().Be(AdvertisementStatus.Sold);
    }

    [Fact]
    public void MarkAsActive_ResetsToActiveFromReserved()
    {
        var ad = Make();
        ad.MarkAsReserved();

        ad.MarkAsActive();

        ad.Status.Should().Be(AdvertisementStatus.Active);
    }

    [Fact]
    public void UpdateDetails_ReplacesFields()
    {
        var ad = Make();

        ad.UpdateDetails("Новий заголовок", "Новий опис", 999m, "Одяг");

        ad.Title.Should().Be("Новий заголовок");
        ad.Price.Should().Be(999m);
        ad.Category.Should().Be("Одяг");
    }

    [Fact]
    public void SetLocation_StoresCoordinates()
    {
        var ad = Make();

        ad.SetLocation(50.45, 30.52, "Київ, Хрещатик");

        ad.Latitude.Should().Be(50.45);
        ad.Longitude.Should().Be(30.52);
        ad.LocationAddress.Should().Be("Київ, Хрещатик");
        ad.HasLocation.Should().BeTrue();
    }

    [Fact]
    public void SetLocation_NullCoords_ClearsLocation()
    {
        var ad = Make();
        ad.SetLocation(50.45, 30.52, "Київ");

        ad.SetLocation(null, null, null);

        ad.HasLocation.Should().BeFalse();
    }

    [Fact]
    public void UpdateSellerSubMerchantId_StoresNonEmptyValue()
    {
        var ad = Make();

        ad.UpdateSellerSubMerchantId("sub_merchant_abc");

        ad.SellerSubMerchantId.Should().Be("sub_merchant_abc");
    }

    [Fact]
    public void UpdateSellerSubMerchantId_WhitespaceBecomesNull()
    {
        var ad = Make();
        ad.UpdateSellerSubMerchantId("sub_abc");

        ad.UpdateSellerSubMerchantId("   ");

        ad.SellerSubMerchantId.Should().BeNull();
    }

    [Fact]
    public void ReplaceImages_OverwritesPreviousList()
    {
        var ad = Make();
        ad.AddImage("old.jpg");

        ad.ReplaceImages(new[] { "a.jpg", "b.jpg" });

        ad.ImageUrls.Should().BeEquivalentTo(new[] { "a.jpg", "b.jpg" });
    }
}
