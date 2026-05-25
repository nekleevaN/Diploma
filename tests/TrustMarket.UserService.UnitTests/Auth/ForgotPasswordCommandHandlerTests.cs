using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Application.Users.Commands.ForgotPassword;
using TrustMarket.UserService.Domain.Entities;
using TrustMarket.UserService.Domain.Repositories;
using Xunit;

namespace TrustMarket.UserService.UnitTests.Auth;

public class ForgotPasswordCommandHandlerTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly IEmailSender _email = Substitute.For<IEmailSender>();
    private readonly IConfiguration _config = Substitute.For<IConfiguration>();

    private ForgotPasswordCommandHandler CreateHandler() =>
        new(_repo, _email, _config);

    private static User MakeEmailUser(string email = "user@mail.com")
    {
        var u = User.Create(email, "user", "Іван", "Іваненко", "hash");
        u.GenerateEmailConfirmationToken();
        // Confirm so provider check passes
        return u;
    }

    [Fact]
    public async Task Handle_UserNotFound_SucceedsWithoutSendingEmail()
    {
        _repo.GetByEmailAsync(Arg.Any<string>(), default).Returns((User?)null);

        var result = await CreateHandler().Handle(new ForgotPasswordCommand("none@mail.com"), default);

        result.IsSuccess.Should().BeTrue();
        await _email.DidNotReceive().SendPasswordResetAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GoogleUser_SucceedsWithoutSendingEmail()
    {
        var googleUser = User.CreateWithGoogle("g@mail.com", "guser", "Анна", "К", "sub");
        _repo.GetByEmailAsync(googleUser.Email, default).Returns(googleUser);

        var result = await CreateHandler().Handle(new ForgotPasswordCommand(googleUser.Email), default);

        result.IsSuccess.Should().BeTrue();
        await _email.DidNotReceive().SendPasswordResetAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmailUser_SendsResetEmailAndSaves()
    {
        var user = MakeEmailUser();
        _repo.GetByEmailAsync(user.Email, default).Returns(user);
        _config["App:BaseUrl"].Returns("http://localhost:3000");

        var result = await CreateHandler().Handle(new ForgotPasswordCommand(user.Email), default);

        result.IsSuccess.Should().BeTrue();
        await _email.Received(1).SendPasswordResetAsync(
            user.Email, user.FirstName,
            Arg.Is<string>(url => url.Contains("reset-password")),
            Arg.Any<CancellationToken>());
        await _repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
