using Microsoft.EntityFrameworkCore;
using TrustMarket.ReviewService.Domain.Entities;

namespace TrustMarket.ReviewService.Infrastructure.Persistence;

public class ReviewDbContext : DbContext
{
    public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options) { }

    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.HasDefaultSchema("reviews");

        mb.Entity<Review>(e =>
        {
            e.ToTable("Reviews");
            e.HasKey(r => r.Id);

            e.Property(r => r.OrderId).IsRequired();
            e.Property(r => r.ReviewerId).IsRequired();
            e.Property(r => r.RevieweeId).IsRequired();
            e.Property(r => r.ReviewerName).HasMaxLength(100).IsRequired();
            e.Property(r => r.Type).HasConversion<int>().IsRequired();
            e.Property(r => r.Status).HasConversion<int>().IsRequired();
            e.Property(r => r.Rating).IsRequired(false);
            e.Property(r => r.Comment).HasMaxLength(500).IsRequired(false);
            e.Property(r => r.IsAnonymous).HasDefaultValue(false);
            e.Property(r => r.DescriptionAccuracy).IsRequired(false);
            e.Property(r => r.ShippingSpeed).IsRequired(false);
            e.Property(r => r.Communication).IsRequired(false);
            e.Property(r => r.PublishedAt).IsRequired(false);
            e.Property(r => r.EditableUntil).IsRequired(false);
            e.Property(r => r.ExpiresAt).IsRequired();
            e.Property(r => r.CreatedAt).IsRequired();
            e.Property(r => r.UpdatedAt).IsRequired(false);

            e.HasIndex(r => new { r.ReviewerId, r.RevieweeId, r.OrderId })
             .IsUnique()
             .HasDatabaseName("IX_Reviews_ReviewerId_RevieweeId_OrderId");

            e.HasIndex(r => new { r.RevieweeId, r.Status })
             .HasDatabaseName("IX_Reviews_RevieweeId_Status");

            e.HasIndex(r => new { r.ReviewerId, r.Status })
             .HasDatabaseName("IX_Reviews_ReviewerId_Status");

            e.HasIndex(r => new { r.Status, r.ExpiresAt })
             .HasDatabaseName("IX_Reviews_Status_ExpiresAt");
        });
    }
}
