using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Application.Users.Commands.RegisterUser;
using TrustMarket.UserService.Domain.Entities;
using TrustMarket.UserService.Domain.Repositories;
using Xunit;

namespace TrustMarket.UserService.UnitTests.Auth;

public class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IEmailSender _emailSender = Substitute.For<IEmailSender>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IPublishEndpoint _publishEndpoint = Substitute.For<IPublishEndpoint>();
    private readonly IConfiguration _config = Substitute.For<IConfiguration>();

    private RegisterUserCommandHandler CreateHandler() =>
        new(_userRepository, _passwordHasher, _emailSender, _tokenGenerator, _publishEndpoint, _config);

    private static RegisterUserCommand ValidCmd(string email = "test@example.com") =>
        new("Test", "User", email, "password123", "password123", true, false, null, null);

    [Fact]
    public async Task Handle_NewUser_ReturnsTokenAndUserId()
    {
        _userRepository.ExistsByEmailAsync("test@example.com", default).Returns(false);
        _userRepository.ExistsByUsernameAsync(Arg.Any<string>(), default).Returns(false);
        _passwordHasher.Hash("password123").Returns("hashed");
        _tokenGenerator.GenerateToken(Arg.Any<User>()).Returns("jwt-token");
        _config["App:BaseUrl"].Returns("http://localhost:3000");

        var result = await CreateHandler().Handle(ValidCmd(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt-token");
        result.Value.UserId.Should().NotBeEmpty();
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), default);
        await _userRepository.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        _userRepository.ExistsByEmailAsync("exists@example.com", default).Returns(true);

        var result = await CreateHandler().Handle(ValidCmd("exists@example.com"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Email");
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), default);
    }

    [Fact]
    public async Task Handle_NewUser_PublishesIntegrationEvent()
    {
        _userRepository.ExistsByEmailAsync(Arg.Any<string>(), default).Returns(false);
        _userRepository.ExistsByUsernameAsync(Arg.Any<string>(), default).Returns(false);
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed");
        _tokenGenerator.GenerateToken(Arg.Any<User>()).Returns("token");
        _config["App:BaseUrl"].Returns("http://localhost:3000");

        await CreateHandler().Handle(ValidCmd("new@example.com"), default);

        await _publishEndpoint.Received(1).Publish(
            Arg.Any<Shared.Contracts.IntegrationEvents.UserRegisteredIntegrationEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NewUser_SendsConfirmationEmail()
    {
        _userRepository.ExistsByEmailAsync(Arg.Any<string>(), default).Returns(false);
        _userRepository.ExistsByUsernameAsync(Arg.Any<string>(), default).Returns(false);
        _passwordHasher.Hash(Arg.Any<string>()).Returns("hashed");
        _tokenGenerator.GenerateToken(Arg.Any<User>()).Returns("token");
        _config["App:BaseUrl"].Returns("http://localhost:3000");

        await CreateHandler().Handle(ValidCmd(), default);

        await _emailSender.Received(1).SendEmailConfirmationAsync(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Is<string>(url => url.Contains("verify-email")),
            Arg.Any<CancellationToken>());
    }
}
