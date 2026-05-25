using Microsoft.EntityFrameworkCore;
using TrustMarket.FinanceService.Domain.Entities;

namespace TrustMarket.FinanceService.Infrastructure.Persistence;

public class FinanceDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();

    public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("finance");

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.AdTitle).HasMaxLength(200).IsRequired();
            entity.Property(o => o.Amount).HasColumnType("numeric(18,2)");
            entity.Property(o => o.InvoiceId).HasMaxLength(100);
            entity.Property(o => o.FailureReason).HasMaxLength(500);
            entity.Property(o => o.Status).HasConversion<int>();
            entity.HasIndex(o => o.BuyerId);
            entity.HasIndex(o => o.SellerId);
            entity.HasIndex(o => o.InvoiceId);
            entity.HasIndex(o => o.AdvertisementId);
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.RecipientCityName).HasMaxLength(100);
            entity.Property(d => d.RecipientWarehouseAddress).HasMaxLength(300);
            entity.Property(d => d.RecipientName).HasMaxLength(100);
            entity.Property(d => d.RecipientPhone).HasMaxLength(20);
            entity.Property(d => d.SenderCityName).HasMaxLength(100);
            entity.Property(d => d.SenderWarehouseAddress).HasMaxLength(300);
            entity.Property(d => d.SenderName).HasMaxLength(100);
            entity.Property(d => d.SenderPhone).HasMaxLength(20);
            entity.Property(d => d.TTN).HasMaxLength(20);
            entity.Property(d => d.TrackingStatusDescription).HasMaxLength(500);
            entity.Property(d => d.Status).HasConversion<int>();
            entity.HasIndex(d => d.OrderId).IsUnique();
            entity.HasOne(d => d.Order)
                  .WithMany()
                  .HasForeignKey(d => d.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
