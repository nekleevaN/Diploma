using FluentAssertions;
using MassTransit;
using NSubstitute;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Application.Users.Commands.ConfirmEmail;
using TrustMarket.UserService.Domain.Entities;
using TrustMarket.UserService.Domain.Repositories;
using Xunit;

namespace TrustMarket.UserService.UnitTests.Auth;

public class ConfirmEmailCommandHandlerTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly IJwtTokenGenerator _jwt = Substitute.For<IJwtTokenGenerator>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();
    private readonly IPublishEndpoint _bus = Substitute.For<IPublishEndpoint>();

    private ConfirmEmailCommandHandler CreateHandler() => new(_repo, _jwt, _email, _bus);

    private static User MakeUnconfirmedUser(out string token)
    {
        var user = User.Create("u@mail.com", "user", "Іван", "І", "hash");
        token = user.GenerateEmailConfirmationToken();
        return user;
    }

    [Fact]
    public async Task Handle_TokenNotFound_ReturnsFailure()
    {
        _repo.GetByEmailConfirmationTokenAsync(Arg.Any<string>(), default).Returns((User?)null);

        var result = await CreateHandler().Handle(new ConfirmEmailCommand("invalid"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("недійсне");
    }

    [Fact]
    public async Task Handle_TokenExpired_ReturnsExpiredError()
    {
        var user = MakeUnconfirmedUser(out var token);
        typeof(User).GetProperty("EmailConfirmationTokenExpiresAt")!
            .SetValue(user, DateTime.UtcNow.AddHours(-2));
        _repo.GetByEmailConfirmationTokenAsync(token, default).Returns(user);

        var result = await CreateHandler().Handle(new ConfirmEmailCommand(token), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("EXPIRED:");
    }

    [Fact]
    public async Task Handle_HappyPath_ConfirmsEmailAndPublishesEventAndSendsWelcome()
    {
        var user = MakeUnconfirmedUser(out var token);
        _repo.GetByEmailConfirmationTokenAsync(token, default).Returns(user);
        _jwt.GenerateToken(user).Returns("jwt-token");

        var result = await CreateHandler().Handle(new ConfirmEmailCommand(token), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.JwtToken.Should().Be("jwt-token");
        user.IsEmailConfirmed.Should().BeTrue();
        await _email.Received(1).SendWelcomeAsync(user.Email, user.FirstName, Arg.Any<CancellationToken>());
        await _bus.Received(1).Publish(
            Arg.Any<UserEmailConfirmedIntegrationEvent>(), Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_HappyPath_TracksBadgeWhenNewlyConfirmed()
    {
        var user = MakeUnconfirmedUser(out var token);
        _repo.GetByEmailConfirmationTokenAsync(token, default).Returns(user);
        _jwt.GenerateToken(user).Returns("jwt-token");

        await CreateHandler().Handle(new ConfirmEmailCommand(token), default);

        _repo.Received(1).TrackBadge(Arg.Is<VerificationBadge>(b => b.Type == BadgeType.EmailVerified));
    }
}
