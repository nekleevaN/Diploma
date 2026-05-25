using Microsoft.EntityFrameworkCore;
using TrustMarket.ChatService.Domain.Entities;

namespace TrustMarket.ChatService.Infrastructure.Persistence;

public class ChatDbContext : DbContext
{
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<ViewingRequest> ViewingRequests => Set<ViewingRequest>();

    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("chat");

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.AdTitle).HasMaxLength(200).IsRequired().HasDefaultValue("Оголошення");
            entity.HasIndex(c => new { c.BuyerId, c.SellerId, c.AdvertisementId }).IsUnique();
            entity.HasMany(c => c.Messages)
                  .WithOne()
                  .HasForeignKey(m => m.ChatId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Content).HasMaxLength(2000).IsRequired();
            entity.Property(m => m.FraudReason).HasMaxLength(500);
        });

        modelBuilder.Entity<ViewingRequest>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.AdTitle).HasMaxLength(200);
            entity.Property(v => v.LocationAddress).HasMaxLength(300);
            entity.Property(v => v.Status).HasConversion<int>();
            entity.Property(v => v.FollowUpAction).HasMaxLength(50);
            entity.HasIndex(v => v.ChatId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
