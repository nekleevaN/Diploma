using Microsoft.EntityFrameworkCore;
using TrustMarket.ChatService.Application.Abstractions;
using TrustMarket.ChatService.Domain.Entities;
using TrustMarket.ChatService.Infrastructure.Persistence;

namespace TrustMarket.ChatService.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly ChatDbContext _context;

    public ChatRepository(ChatDbContext context) => _context = context;

    public Task<Chat?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Chats.Include(c => c.Messages).FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Chat?> GetByParticipantsValidation(Guid chatId, CancellationToken ct = default)
        => _context.Chats.AsNoTracking().FirstOrDefaultAsync(c => c.Id == chatId, ct);

    public Task<Chat?> GetByParticipantsAndAdAsync(Guid buyerId, Guid sellerId, Guid advertisementId, CancellationToken ct = default)
        => _context.Chats.FirstOrDefaultAsync(
            c => c.BuyerId == buyerId && c.SellerId == sellerId && c.AdvertisementId == advertisementId, ct);

    public Task<List<Chat>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _context.Chats.Include(c => c.Messages)
            .Where(c => c.BuyerId == userId || c.SellerId == userId)
            .ToListAsync(ct);

    public async Task AddAsync(Chat chat, CancellationToken ct = default)
        => await _context.Chats.AddAsync(chat, ct);

    public async Task SaveMessageAsync(Message message, CancellationToken ct = default)
    {
        await _context.Messages.AddAsync(message, ct);
        await _context.SaveChangesAsync(ct);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
