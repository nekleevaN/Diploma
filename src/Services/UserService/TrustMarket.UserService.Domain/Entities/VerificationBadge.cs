using TrustMarket.Shared.Common.Domain;

namespace TrustMarket.UserService.Domain.Entities;

public class VerificationBadge : BaseEntity
{
    public Guid UserId { get; private set; }
    public BadgeType Type { get; private set; }
    public DateTime IssuedAt { get; private set; }

    private VerificationBadge() { }

    public static VerificationBadge Create(Guid userId, BadgeType type)
    {
        return new VerificationBadge
        {
            UserId = userId,
            Type = type,
            IssuedAt = DateTime.UtcNow
        };
    }
}

public enum BadgeType
{
    EmailVerified = 1,
    PhoneVerified = 2,
    DiiaVerified = 3,
    TrustedSeller = 4
}
