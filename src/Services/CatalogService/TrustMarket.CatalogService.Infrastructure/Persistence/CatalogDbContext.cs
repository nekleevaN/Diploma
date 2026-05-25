using Microsoft.EntityFrameworkCore;
using TrustMarket.CatalogService.Domain.Entities;

namespace TrustMarket.CatalogService.Infrastructure.Persistence;

public class CatalogDbContext : DbContext
{
    public DbSet<Advertisement> Advertisements => Set<Advertisement>();
    public DbSet<Offer> Offers => Set<Offer>();

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");

        modelBuilder.Entity<Advertisement>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Title).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Description).HasMaxLength(5000).IsRequired();
            entity.Property(a => a.Price).HasColumnType("numeric(18,2)");
            entity.Property(a => a.Category).HasMaxLength(100).IsRequired();
            entity.Property(a => a.CategorySub).HasMaxLength(100);
            entity.Property(a => a.CategoryItem).HasMaxLength(100);
            entity.Property(a => a.CategoryLabel).HasMaxLength(300);
            entity.Property(a => a.Condition).HasMaxLength(50);
            entity.Property(a => a.Brand).HasMaxLength(100);
            entity.Property(a => a.Size).HasMaxLength(30);
            entity.Property(a => a.Color).HasMaxLength(50);
            entity.Property(a => a.SellerName).HasMaxLength(50).IsRequired();
            entity.Property(a => a.Status).HasConversion<int>();
            entity.Property(a => a.LocationAddress).HasMaxLength(300);

            entity.HasIndex(a => a.Category);
            entity.HasIndex(a => new { a.Category, a.CategorySub });
            entity.HasIndex(a => a.Status);
        });

        modelBuilder.Entity<Offer>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.BuyerName).HasMaxLength(50).IsRequired();
            entity.Property(o => o.OfferedPrice).HasColumnType("numeric(18,2)");
            entity.Property(o => o.CounterPrice).HasColumnType("numeric(18,2)");
            entity.Property(o => o.SellerNote).HasMaxLength(500);
            entity.Property(o => o.Status).HasConversion<int>();
            entity.HasIndex(o => o.AdvertisementId);
            entity.HasIndex(o => o.BuyerId);
            entity.HasOne<Advertisement>()
                  .WithMany()
                  .HasForeignKey(o => o.AdvertisementId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
