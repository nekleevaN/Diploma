using System.Security.Cryptography;
using TrustMarket.Shared.Common.Domain;

namespace TrustMarket.UserService.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = null!;
    public string Username { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;

    public string? PasswordHash { get; private set; }
    public AuthProvider AuthProvider { get; private set; } = AuthProvider.Email;
    public string? ExternalId { get; private set; }

    public bool IsEmailConfirmed { get; private set; }
    public string? EmailConfirmationToken { get; private set; }
    public DateTime? EmailConfirmationTokenExpiresAt { get; private set; }

    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    public string? PhoneNumber { get; private set; }
    public bool IsPhoneConfirmed { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }
    public long? TrustedContactTelegramId { get; private set; }
    public string? TrustedContactEmail { get; private set; }

    public string? MonobankSubMerchantId { get; private set; }
    public PublicNameMode PublicNameMode { get; private set; } = PublicNameMode.FirstNameAndInitial;
    public DateTime? LastNameChangedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public int FraudReports { get; private set; }
    public double Rating { get; private set; } = 5.0;
    public double SellerRating { get; private set; }
    public int SellerReviewCount { get; private set; }
    public double BuyerRating { get; private set; }
    public int BuyerReviewCount { get; private set; }

    private readonly List<VerificationBadge> _badges = new();
    public IReadOnlyCollection<VerificationBadge> Badges => _badges.AsReadOnly();

    public string FullName => $"{FirstName} {LastName}";

    public string DisplayName => PublicNameMode switch
    {
        PublicNameMode.FirstNameOnly       => FirstName,
        PublicNameMode.FirstNameAndInitial => $"{FirstName} {LastName[0]}.",
        PublicNameMode.FullName            => FullName,
        _                                  => FirstName
    };

    private User() { }


    public static User Create(
        string email, string username,
        string firstName, string lastName,
        string passwordHash)
        => new()
        {
            Email        = email,
            Username     = username,
            FirstName    = firstName.Trim(),
            LastName     = lastName.Trim(),
            PasswordHash = passwordHash,
            AuthProvider = AuthProvider.Email,
            IsEmailConfirmed = false,
            PublicNameMode   = PublicNameMode.FirstNameAndInitial
        };

    public static User CreateWithGoogle(
        string email, string username,
        string firstName, string lastName,
        string externalId)
        => new()
        {
            Email        = email,
            Username     = username,
            FirstName    = firstName.Trim(),
            LastName     = lastName.Trim(),
            AuthProvider = AuthProvider.Google,
            ExternalId   = externalId,
            IsEmailConfirmed = true,
            PublicNameMode   = PublicNameMode.FirstNameAndInitial
        };


    public string GenerateEmailConfirmationToken()
    {
        var token = GenerateSecureToken();
        EmailConfirmationToken = token;
        EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        UpdatedAt = DateTime.UtcNow;
        return token;
    }

    public (bool Success, VerificationBadge? NewBadge) TryConfirmEmail(string token)
    {
        if (IsEmailConfirmed) return (true, null);
        if (EmailConfirmationToken != token) return (false, null);
        if (EmailConfirmationTokenExpiresAt < DateTime.UtcNow) return (false, null);

        IsEmailConfirmed = true;
        EmailConfirmationToken = null;
        EmailConfirmationTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;

        if (!HasBadge(BadgeType.EmailVerified))
        {
            var badge = VerificationBadge.Create(Id, BadgeType.EmailVerified);
            _badges.Add(badge);
            return (true, badge);
        }
        return (true, null);
    }

    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
        EmailConfirmationToken = null;
        EmailConfirmationTokenExpiresAt = null;
        AddBadge(BadgeType.EmailVerified);
        UpdatedAt = DateTime.UtcNow;
    }


    public string GeneratePasswordResetToken()
    {
        var token = GenerateSecureToken();
        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        UpdatedAt = DateTime.UtcNow;
        return token;
    }

    public bool TryResetPassword(string token, string newPasswordHash)
    {
        if (PasswordResetToken != token) return false;
        if (PasswordResetTokenExpiresAt < DateTime.UtcNow) return false;

        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
        return true;
    }


    public (bool Ok, string? Error) UpdateName(string firstName, string lastName)
    {
        if (LastNameChangedAt.HasValue &&
            (DateTime.UtcNow - LastNameChangedAt.Value).TotalDays < 30)
        {
            var next = LastNameChangedAt.Value.AddDays(30);
            return (false, $"Ім'я можна змінювати раз на 30 днів. Наступна зміна: {next:dd.MM.yyyy}");
        }

        FirstName = firstName.Trim();
        LastName  = lastName.Trim();
        LastNameChangedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        return (true, null);
    }

    public void UpdatePublicNameMode(PublicNameMode mode)
    {
        PublicNameMode = mode;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMonobankSubMerchantId(string? subMerchantId)
    {
        MonobankSubMerchantId = string.IsNullOrWhiteSpace(subMerchantId) ? null : subMerchantId.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(
        string? bio,
        long? trustedContactTelegramId = null,
        string? trustedContactEmail = null)
    {
        Bio = bio?.Trim();
        if (trustedContactTelegramId.HasValue)
            TrustedContactTelegramId = trustedContactTelegramId;
        if (trustedContactEmail is not null)
            TrustedContactEmail = string.IsNullOrWhiteSpace(trustedContactEmail)
                ? null : trustedContactEmail.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }


    public void ConfirmPhone(string phoneNumber)
    {
        PhoneNumber = phoneNumber;
        IsPhoneConfirmed = true;
        AddBadge(BadgeType.PhoneVerified);
        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyViaDiia(string fullName, string taxId)
    {
        AddBadge(BadgeType.DiiaVerified);
        UpdatedAt = DateTime.UtcNow;
    }


    public void UpdateReviewRating(bool asSeller, int newRating)
    {
        if (asSeller)
        {
            SellerRating = Math.Round(
                (SellerRating * SellerReviewCount + newRating) / (SellerReviewCount + 1), 2);
            SellerReviewCount++;
        }
        else
        {
            BuyerRating = Math.Round(
                (BuyerRating * BuyerReviewCount + newRating) / (BuyerReviewCount + 1), 2);
            BuyerReviewCount++;
        }

        var totalCount = SellerReviewCount + BuyerReviewCount;
        Rating = totalCount > 0
            ? Math.Round((SellerRating * SellerReviewCount + BuyerRating * BuyerReviewCount) / totalCount, 2)
            : 5.0;

        UpdatedAt = DateTime.UtcNow;
    }

    public void ReportAsFraudulent()
    {
        FraudReports++;
        Rating = Math.Max(0, Rating - 0.5);
        UpdatedAt = DateTime.UtcNow;
    }


    public bool HasBadge(BadgeType type) => _badges.Any(b => b.Type == type);

    private void AddBadge(BadgeType type)
    {
        if (HasBadge(type)) return;
        _badges.Add(VerificationBadge.Create(Id, type));
    }

    private static string GenerateSecureToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
               .Replace('+', '-').Replace('/', '_').TrimEnd('=');
}
