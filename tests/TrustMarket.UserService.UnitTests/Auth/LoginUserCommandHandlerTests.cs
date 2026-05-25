using FluentAssertions;
using NSubstitute;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Application.Users.Commands.LoginUser;
using TrustMarket.UserService.Domain.Entities;
using TrustMarket.UserService.Domain.Repositories;
using Xunit;

namespace TrustMarket.UserService.UnitTests.Auth;

public class LoginUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();

    private LoginUserCommandHandler CreateHandler() =>
        new(_userRepository, _passwordHasher, _tokenGenerator);

    private static User CreateUser(string email = "test@example.com", string hash = "hashed")
    {
        var user = User.Create(email, "testuser", "Test", "User", hash);
        var token = user.GenerateEmailConfirmationToken();
        user.TryConfirmEmail(token);
        return user;
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        var user = CreateUser();
        _userRepository.GetByEmailAsync("test@example.com", default).Returns(user);
        _passwordHasher.Verify("password123", "hashed").Returns(true);
        _tokenGenerator.GenerateToken(user).Returns("jwt-token");

        var result = await CreateHandler().Handle(
            new LoginUserCommand("test@example.com", "password123"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt-token");
        result.Value.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_UnknownEmail_ReturnsFailure()
    {
        _userRepository.GetByEmailAsync("nobody@example.com", default).Returns((User?)null);

        var result = await CreateHandler().Handle(
            new LoginUserCommand("nobody@example.com", "password"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Невірний");
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsFailure()
    {
        var user = CreateUser();
        _userRepository.GetByEmailAsync("test@example.com", default).Returns(user);
        _passwordHasher.Verify("wrongpass", "hashed").Returns(false);

        var result = await CreateHandler().Handle(
            new LoginUserCommand("test@example.com", "wrongpass"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Невірний");
        _tokenGenerator.DidNotReceive().GenerateToken(Arg.Any<User>());
    }
}
