using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using TrustMarket.Shared.Common.Results;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Domain.Repositories;

namespace TrustMarket.UserService.Application.Users.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IUserRepository _repo;
    private readonly IEmailSender _email;
    private readonly IConfiguration _config;

    public ForgotPasswordCommandHandler(
        IUserRepository repo, IEmailSender email, IConfiguration config)
    {
        _repo   = repo;
        _email  = email;
        _config = config;
    }

    public async Task<Result> Handle(ForgotPasswordCommand req, CancellationToken ct)
    {
        var user = await _repo.GetByEmailAsync(req.Email, ct);
        if (user is null || user.AuthProvider != Domain.Entities.AuthProvider.Email)
            return Result.Success();

        var token = user.GeneratePasswordResetToken();
        await _repo.SaveChangesAsync(ct);

        var baseUrl = _config["App:BaseUrl"] ?? "http://localhost:3000";
        var resetUrl = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(token)}";

        await _email.SendPasswordResetAsync(user.Email, user.FirstName, resetUrl, ct);

        return Result.Success();
    }
}
