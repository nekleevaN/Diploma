using FluentAssertions;
using NSubstitute;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Application.Users.Commands.ResetPassword;
using TrustMarket.UserService.Domain.Entities;
using TrustMarket.UserService.Domain.Repositories;
using Xunit;

namespace TrustMarket.UserService.UnitTests.Auth;

public class ResetPasswordCommandHandlerTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwt = Substitute.For<IJwtTokenGenerator>();

    private ResetPasswordCommandHandler CreateHandler() => new(_repo, _hasher, _jwt);

    private static User MakeUserWithResetToken(out string token)
    {
        var user = User.Create("u@mail.com", "user", "Іван", "І", "old-hash");
        token = user.GeneratePasswordResetToken();
        return user;
    }

    [Fact]
    public async Task Handle_TokenNotFound_ReturnsFailure()
    {
        _repo.GetByPasswordResetTokenAsync(Arg.Any<string>(), default).Returns((User?)null);

        var result = await CreateHandler().Handle(
            new ResetPasswordCommand("bad-token", "NewPass1", "NewPass1"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("недійсне");
    }

    [Fact]
    public async Task Handle_HappyPath_ResetsPasswordAndReturnsJwt()
    {
        var user = MakeUserWithResetToken(out var token);
        _repo.GetByPasswordResetTokenAsync(token, default).Returns(user);
        _hasher.Hash("NewPass1").Returns("new-hash");
        _jwt.GenerateToken(user).Returns("jwt-token");

        var result = await CreateHandler().Handle(
            new ResetPasswordCommand(token, "NewPass1", "NewPass1"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.JwtToken.Should().Be("jwt-token");
        user.PasswordHash.Should().Be("new-hash");
        user.PasswordResetToken.Should().BeNull();
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_TokenExpired_ReturnsExpiredError()
    {
        var user = MakeUserWithResetToken(out var token);
        // Manually corrupt expiry via reflection — expired by 2 hours
        var expiredAt = DateTime.UtcNow.AddHours(-2);
        typeof(User).GetProperty("PasswordResetTokenExpiresAt")!
            .SetValue(user, expiredAt);
        _repo.GetByPasswordResetTokenAsync(token, default).Returns(user);

        var result = await CreateHandler().Handle(
            new ResetPasswordCommand(token, "NewPass1", "NewPass1"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("EXPIRED:");
    }
}
