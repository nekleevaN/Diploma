namespace TrustMarket.Shared.Common.Domain;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
