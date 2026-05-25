using TrustMarket.ChatService.Domain.Entities;

namespace TrustMarket.ChatService.Application.Abstractions;

public interface IChatRepository
{
    Task<Chat?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Chat?> GetByParticipantsValidation(Guid chatId, CancellationToken ct = default);
    Task<Chat?> GetByParticipantsAndAdAsync(Guid buyerId, Guid sellerId, Guid advertisementId, CancellationToken ct = default);
    Task<List<Chat>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Chat chat, CancellationToken ct = default);
    Task SaveMessageAsync(Message message, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
