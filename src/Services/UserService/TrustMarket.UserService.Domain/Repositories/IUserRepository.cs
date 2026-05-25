using TrustMarket.UserService.Domain.Entities;

namespace TrustMarket.UserService.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByEmailConfirmationTokenAsync(string token, CancellationToken ct = default);
    Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
    void TrackBadge(VerificationBadge badge);
    Task AddBadgeAsync(VerificationBadge badge, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
