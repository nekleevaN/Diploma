using FluentValidation;
using MediatR;
using TrustMarket.Shared.Common.Results;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Domain.Repositories;

namespace TrustMarket.UserService.Application.Users.Commands.ResetPassword;

public record ResetPasswordCommand(
    string Token,
    string NewPassword,
    string ConfirmPassword) : IRequest<Result<ResetPasswordResponse>>;

public record ResetPasswordResponse(Guid UserId, string JwtToken);

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Пароль має бути не менше 8 символів")
            .Matches(@"\d").WithMessage("Пароль має містити хоча б одну цифру")
            .Matches(@"[a-zA-Z]").WithMessage("Пароль має містити хоча б одну літеру");
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("Паролі не співпадають");
    }
}

public class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand, Result<ResetPasswordResponse>>
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;

    public ResetPasswordCommandHandler(
        IUserRepository repo, IPasswordHasher hasher, IJwtTokenGenerator jwt)
    {
        _repo   = repo;
        _hasher = hasher;
        _jwt    = jwt;
    }

    public async Task<Result<ResetPasswordResponse>> Handle(
        ResetPasswordCommand req, CancellationToken ct)
    {
        var user = await _repo.GetByPasswordResetTokenAsync(req.Token, ct);

        if (user is null)
            return Result.Failure<ResetPasswordResponse>("Посилання недійсне або вже використане");

        if (user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
            return Result.Failure<ResetPasswordResponse>(
                "EXPIRED:Посилання для скидання паролю застаріло. Запросіть нове.");

        var newHash = _hasher.Hash(req.NewPassword);
        if (!user.TryResetPassword(req.Token, newHash))
            return Result.Failure<ResetPasswordResponse>("Посилання недійсне");

        await _repo.SaveChangesAsync(ct);

        var token = _jwt.GenerateToken(user);
        return Result.Success(new ResetPasswordResponse(user.Id, token));
    }
}
