using FluentAssertions;
using NSubstitute;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Application.Users.Commands.VerifyDiia;
using TrustMarket.UserService.Domain.Entities;
using TrustMarket.UserService.Domain.Repositories;
using Xunit;

namespace TrustMarket.UserService.UnitTests.Auth;

public class DiiaVerificationCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IDiiaService _diiaService = Substitute.For<IDiiaService>();

    private StartDiiaVerificationCommandHandler CreateStartHandler() =>
        new(_userRepository, _diiaService);

    private ConfirmDiiaVerificationCommandHandler CreateConfirmHandler() =>
        new(_userRepository, _diiaService);

    [Fact]
    public async Task Start_ExistingUser_ReturnsSessionId()
    {
        var user = User.Create("test@example.com", "testuser", "Test", "User", "hash");
        _userRepository.GetByIdAsync(user.Id, default).Returns(user);
        _diiaService.StartVerificationAsync(user.Id, default).Returns("session-123");

        var result = await CreateStartHandler().Handle(
            new StartDiiaVerificationCommand(user.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.SessionId.Should().Be("session-123");
    }

    [Fact]
    public async Task Start_UnknownUser_ReturnsFailure()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>(), default).Returns((User?)null);

        var result = await CreateStartHandler().Handle(
            new StartDiiaVerificationCommand(Guid.NewGuid()), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("не знайдено");
    }

    [Fact]
    public async Task Start_AlreadyVerified_ReturnsFailure()
    {
        var user = User.Create("test@example.com", "testuser", "Test", "User", "hash");
        user.VerifyViaDiia("Тест Тестовий", "1234567890");
        _userRepository.GetByIdAsync(user.Id, default).Returns(user);

        var result = await CreateStartHandler().Handle(
            new StartDiiaVerificationCommand(user.Id), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().ContainEquivalentOf("верифіковано");
    }

    [Fact]
    public async Task Confirm_ValidSession_SetsVerifiedBadge()
    {
        var user = User.Create("test@example.com", "testuser", "Test", "User", "hash");
        var sessionId = "valid-session";
        var diiaResult = new DiiaVerificationResult("Іван Тест", "1234567890", new DateTime(1990, 1, 1));

        _diiaService.VerifyAsync(sessionId, default).Returns(diiaResult);
        _userRepository.GetByIdAsync(user.Id, default).Returns(user);

        var result = await CreateConfirmHandler().Handle(
            new ConfirmDiiaVerificationCommand(user.Id, sessionId), default);

        result.IsSuccess.Should().BeTrue();
        await _userRepository.Received(1).AddBadgeAsync(
            Arg.Is<VerificationBadge>(b => b.Type == BadgeType.DiiaVerified),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Confirm_InvalidSession_ReturnsFailure()
    {
        _diiaService.VerifyAsync("bad-session", default).Returns((DiiaVerificationResult?)null);

        var result = await CreateConfirmHandler().Handle(
            new ConfirmDiiaVerificationCommand(Guid.NewGuid(), "bad-session"), default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Сесія");
    }
}
