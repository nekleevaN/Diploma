using MediatR;
using Microsoft.Extensions.Configuration;
using TrustMarket.Shared.Common.Results;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Domain.Repositories;

namespace TrustMarket.UserService.Application.Users.Commands.ResendVerification;

public record ResendVerificationCommand(Guid UserId) : IRequest<Result>;

public class ResendVerificationCommandHandler
    : IRequestHandler<ResendVerificationCommand, Result>
{
    private readonly IUserRepository _repo;
    private readonly IEmailSender _email;
    private readonly IConfiguration _config;

    public ResendVerificationCommandHandler(
        IUserRepository repo, IEmailSender email, IConfiguration config)
    {
        _repo   = repo;
        _email  = email;
        _config = config;
    }

    public async Task<Result> Handle(ResendVerificationCommand req, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(req.UserId, ct);
        if (user is null) return Result.Failure("Користувача не знайдено");
        if (user.IsEmailConfirmed) return Result.Failure("Email вже підтверджено");

        if (user.EmailConfirmationTokenExpiresAt.HasValue)
        {
            var tokenCreatedAt = user.EmailConfirmationTokenExpiresAt.Value.AddHours(-24);
            if ((DateTime.UtcNow - tokenCreatedAt).TotalSeconds < 60)
                return Result.Failure("RATE_LIMIT:Зачекайте 60 секунд перед повторним запитом");
        }

        var confirmToken = user.GenerateEmailConfirmationToken();
        await _repo.SaveChangesAsync(ct);

        var baseUrl = _config["App:BaseUrl"] ?? "http://localhost:3000";
        var confirmUrl = $"{baseUrl}/verify-email?token={Uri.EscapeDataString(confirmToken)}";
        await _email.SendEmailConfirmationAsync(user.Email, user.FirstName, confirmUrl, ct);

        return Result.Success();
    }
}
