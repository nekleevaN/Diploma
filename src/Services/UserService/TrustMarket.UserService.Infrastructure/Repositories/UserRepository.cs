using Microsoft.EntityFrameworkCore;
using TrustMarket.UserService.Domain.Entities;
using TrustMarket.UserService.Domain.Repositories;
using TrustMarket.UserService.Infrastructure.Persistence;

namespace TrustMarket.UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context) => _context = context;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Users.Include(u => u.Badges).FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _context.Users.Include(u => u.Badges).FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> GetByEmailConfirmationTokenAsync(string token, CancellationToken ct = default)
        => _context.Users.Include(u => u.Badges)
            .FirstOrDefaultAsync(u => u.EmailConfirmationToken == token, ct);

    public Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken ct = default)
        => _context.Users.Include(u => u.Badges)
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => _context.Users.AnyAsync(u => u.Email == email, ct);

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
        => _context.Users.AnyAsync(u => u.Username == username, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _context.Users.AddAsync(user, ct);

    public void Update(User user) => _context.Users.Update(user);

    public void TrackBadge(VerificationBadge badge)
        => _context.VerificationBadges.Add(badge);

    public async Task AddBadgeAsync(VerificationBadge badge, CancellationToken ct = default)
    {
        await _context.VerificationBadges.AddAsync(badge, ct);
        await _context.SaveChangesAsync(ct);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
