using Microsoft.EntityFrameworkCore;
using TrustMarket.UserService.Domain.Entities;

namespace TrustMarket.UserService.Infrastructure.Persistence;

public class UserDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<VerificationBadge> VerificationBadges => Set<VerificationBadge>();

    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("users");

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();

            entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
            entity.Property(u => u.Username).HasMaxLength(50).IsRequired();
            entity.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(50).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(500);
            entity.Property(u => u.AuthProvider).HasConversion<int>().HasDefaultValue(Domain.Entities.AuthProvider.Email);
            entity.Property(u => u.ExternalId).HasMaxLength(200);

            entity.Property(u => u.TrustedContactEmail).HasMaxLength(255);
            entity.Property(u => u.MonobankSubMerchantId).HasMaxLength(100);
            entity.Property(u => u.EmailConfirmationToken).HasMaxLength(100);
            entity.Property(u => u.PasswordResetToken).HasMaxLength(100);

            entity.Property(u => u.PhoneNumber).HasMaxLength(20);
            entity.Property(u => u.AvatarUrl).HasMaxLength(500);
            entity.Property(u => u.Bio).HasMaxLength(500);
            entity.Property(u => u.PublicNameMode).HasConversion<int>()
                  .HasDefaultValue(Domain.Entities.PublicNameMode.FirstNameAndInitial);

            entity.Property(u => u.SellerRating).HasDefaultValue(0.0);
            entity.Property(u => u.SellerReviewCount).HasDefaultValue(0);
            entity.Property(u => u.BuyerRating).HasDefaultValue(0.0);
            entity.Property(u => u.BuyerReviewCount).HasDefaultValue(0);

            entity.Ignore(u => u.FullName);
            entity.Ignore(u => u.DisplayName);

            entity.HasMany(u => u.Badges)
                  .WithOne()
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VerificationBadge>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Type).HasConversion<int>();
        });

        base.OnModelCreating(modelBuilder);
    }
}
