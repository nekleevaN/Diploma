using FluentAssertions;
using NSubstitute;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Application.Users.Commands.GoogleAuth;
using TrustMarket.UserService.Domain.Entities;
using TrustMarket.UserService.Domain.Repositories;
using Xunit;

namespace TrustMarket.UserService.UnitTests.Auth;

public class GoogleAuthCommandHandlerTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly IJwtTokenGenerator _jwt = Substitute.For<IJwtTokenGenerator>();
    private readonly IGoogleTokenValidator _google = Substitute.For<IGoogleTokenValidator>();

    private GoogleAuthCommandHandler CreateHandler() => new(_repo, _jwt, _google);

    private static GooglePayload ValidPayload() =>
        new("sub_google_123", "user@gmail.com", "Анна", "Коваль");

    [Fact]
    public async Task Handle_InvalidGoogleToken_ReturnsFailure()
    {
        _google.ValidateAsync(Arg.Any<string>(), default).Returns((GooglePayload?)null);

        var result = await CreateHandler().Handle(new GoogleAuthCommand("bad-token"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Недійсний");
    }

    [Fact]
    public async Task Handle_EmailRegisteredWithPassword_ReturnsConflictError()
    {
        var payload = ValidPayload();
        _google.ValidateAsync(Arg.Any<string>(), default).Returns(payload);
        var emailUser = User.Create(payload.Email, "user", "Анна", "К", "hash");
        _repo.GetByEmailAsync(payload.Email, default).Returns(emailUser);

        var result = await CreateHandler().Handle(new GoogleAuthCommand("token"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().StartWith("CONFLICT:");
    }

    [Fact]
    public async Task Handle_ExistingGoogleUser_RecordsLoginAndReturnsToken()
    {
        var payload = ValidPayload();
        _google.ValidateAsync(Arg.Any<string>(), default).Returns(payload);
        var existing = User.CreateWithGoogle(payload.Email, "user", "Анна", "К", payload.Subject);
        _repo.GetByEmailAsync(payload.Email, default).Returns(existing);
        _jwt.GenerateToken(existing).Returns("jwt-existing");

        var result = await CreateHandler().Handle(new GoogleAuthCommand("token"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsNewUser.Should().BeFalse();
        result.Value.JwtToken.Should().Be("jwt-existing");
        _repo.Received(1).Update(existing);
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NewGoogleUser_CreatesUserAndReturnsIsNewTrue()
    {
        var payload = ValidPayload();
        _google.ValidateAsync(Arg.Any<string>(), default).Returns(payload);
        _repo.GetByEmailAsync(payload.Email, default).Returns((User?)null);
        _repo.ExistsByUsernameAsync(Arg.Any<string>(), default).Returns(false);
        _jwt.GenerateToken(Arg.Any<User>()).Returns("jwt-new");

        var result = await CreateHandler().Handle(new GoogleAuthCommand("token"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsNewUser.Should().BeTrue();
        result.Value.JwtToken.Should().Be("jwt-new");
        await _repo.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == payload.Email && u.AuthProvider == AuthProvider.Google),
            Arg.Any<CancellationToken>());
    }
}
