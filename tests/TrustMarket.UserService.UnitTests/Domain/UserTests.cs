using FluentAssertions;
using TrustMarket.UserService.Domain.Entities;
using Xunit;

namespace TrustMarket.UserService.UnitTests.Domain;

public class UserTests
{
    private static User MakeEmailUser() =>
        User.Create("test@mail.com", "testuser", "Іван", "Іваненко", "hashed");

    [Fact]
    public void Create_SetsEmailProviderAndUnconfirmedEmail()
    {
        var user = MakeEmailUser();

        user.AuthProvider.Should().Be(AuthProvider.Email);
        user.IsEmailConfirmed.Should().BeFalse();
        user.PasswordHash.Should().Be("hashed");
    }

    [Fact]
    public void CreateWithGoogle_SetsGoogleProviderAndConfirmsEmail()
    {
        var user = User.CreateWithGoogle("g@mail.com", "guser", "Анна", "Коваль", "sub_123");

        user.AuthProvider.Should().Be(AuthProvider.Google);
        user.IsEmailConfirmed.Should().BeTrue();
        user.ExternalId.Should().Be("sub_123");
        user.PasswordHash.Should().BeNull();
    }

    [Fact]
    public void GenerateEmailConfirmationToken_ReturnsNonEmptyTokenAndSetsExpiry()
    {
        var user = MakeEmailUser();

        var token = user.GenerateEmailConfirmationToken();

        token.Should().NotBeNullOrEmpty();
        user.EmailConfirmationToken.Should().Be(token);
        user.EmailConfirmationTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void TryConfirmEmail_ValidToken_ConfirmsEmailAndAddsBadge()
    {
        var user = MakeEmailUser();
        var token = user.GenerateEmailConfirmationToken();

        var (ok, badge) = user.TryConfirmEmail(token);

        ok.Should().BeTrue();
        user.IsEmailConfirmed.Should().BeTrue();
        badge.Should().NotBeNull();
        badge!.Type.Should().Be(BadgeType.EmailVerified);
    }

    [Fact]
    public void TryConfirmEmail_WrongToken_ReturnsFalse()
    {
        var user = MakeEmailUser();
        user.GenerateEmailConfirmationToken();

        var (ok, badge) = user.TryConfirmEmail("wrong-token");

        ok.Should().BeFalse();
        badge.Should().BeNull();
        user.IsEmailConfirmed.Should().BeFalse();
    }

    [Fact]
    public void TryConfirmEmail_AlreadyConfirmed_ReturnsTrueWithoutBadge()
    {
        var user = MakeEmailUser();
        var token = user.GenerateEmailConfirmationToken();
        user.TryConfirmEmail(token);

        var (ok, badge) = user.TryConfirmEmail(token);

        ok.Should().BeTrue();
        badge.Should().BeNull();
    }

    [Fact]
    public void GeneratePasswordResetToken_ReturnsTokenWithExpiry()
    {
        var user = MakeEmailUser();

        var token = user.GeneratePasswordResetToken();

        token.Should().NotBeNullOrEmpty();
        user.PasswordResetToken.Should().Be(token);
        user.PasswordResetTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void TryResetPassword_ValidToken_UpdatesHashAndClearsToken()
    {
        var user = MakeEmailUser();
        var token = user.GeneratePasswordResetToken();

        var ok = user.TryResetPassword(token, "new-hash");

        ok.Should().BeTrue();
        user.PasswordHash.Should().Be("new-hash");
        user.PasswordResetToken.Should().BeNull();
    }

    [Fact]
    public void TryResetPassword_WrongToken_ReturnsFalse()
    {
        var user = MakeEmailUser();
        user.GeneratePasswordResetToken();

        var ok = user.TryResetPassword("bad-token", "new-hash");

        ok.Should().BeFalse();
        user.PasswordHash.Should().Be("hashed");
    }

    [Fact]
    public void UpdateName_FirstChange_Succeeds()
    {
        var user = MakeEmailUser();

        var (ok, error) = user.UpdateName("Петро", "Петренко");

        ok.Should().BeTrue();
        error.Should().BeNull();
        user.FirstName.Should().Be("Петро");
        user.LastName.Should().Be("Петренко");
    }

    [Fact]
    public void UpdateName_TwiceWithin30Days_ReturnsError()
    {
        var user = MakeEmailUser();
        user.UpdateName("Петро", "Петренко");

        var (ok, error) = user.UpdateName("Олег", "Оленко");

        ok.Should().BeFalse();
        error.Should().Contain("30 днів");
    }

    [Fact]
    public void UpdateReviewRating_AsSeller_UpdatesSellerStatsCorrectly()
    {
        var user = MakeEmailUser();

        user.UpdateReviewRating(asSeller: true, 4);
        user.UpdateReviewRating(asSeller: true, 2);

        user.SellerReviewCount.Should().Be(2);
        user.SellerRating.Should().Be(3.0);
    }

    [Fact]
    public void SetMonobankSubMerchantId_WhitespaceClearsToNull()
    {
        var user = MakeEmailUser();
        user.SetMonobankSubMerchantId("sub_abc");

        user.SetMonobankSubMerchantId("  ");

        user.MonobankSubMerchantId.Should().BeNull();
    }

    [Fact]
    public void DisplayName_FirstNameAndInitialMode_FormatsCorrectly()
    {
        var user = MakeEmailUser();

        user.DisplayName.Should().Be("Іван І.");
    }
}
