using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using TrustMarket.Shared.Common.Results;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Application.Users.Helpers;
using TrustMarket.UserService.Domain.Entities;
using TrustMarket.UserService.Domain.Repositories;

namespace TrustMarket.UserService.Application.Users.Commands.RegisterUser;

public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PasswordConfirm,
    bool AgreeToTerms,
    bool WantsNewsletter,
    string? Website,
    long? FormOpenedAt
) : IRequest<Result<RegisterUserResponse>>;

public record RegisterUserResponse(Guid UserId, string Token, string Message);

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private static readonly HashSet<string> PasswordBlacklist = new(StringComparer.OrdinalIgnoreCase)
    {
        "password","password1","123456","12345678","qwerty","qwerty123",
        "111111","123123","abc123","letmein","monkey","master","dragon",
        "sunshine","princess","welcome","shadow","superman","michael",
        "password123","admin","login","pass","test","root","toor"
    };

    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ім'я обов'язкове")
            .MinimumLength(2).MaximumLength(50)
            .Matches(@"^[\p{L}\s\-']+$").WithMessage("Ім'я може містити лише літери, пробіл і дефіс");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Прізвище обов'язкове")
            .MinimumLength(2).MaximumLength(50)
            .Matches(@"^[\p{L}\s\-']+$").WithMessage("Прізвище може містити лише літери, пробіл і дефіс");

        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress().WithMessage("Некоректний email");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Пароль має бути не менше 8 символів")
            .Matches(@"\d").WithMessage("Пароль має містити хоча б одну цифру")
            .Matches(@"[a-zA-Z]").WithMessage("Пароль має містити хоча б одну літеру")
            .Must(p => !PasswordBlacklist.Contains(p))
                .WithMessage("Цей пароль занадто простий. Оберіть надійніший.");

        RuleFor(x => x.PasswordConfirm)
            .Equal(x => x.Password).WithMessage("Паролі не співпадають");

        RuleFor(x => x.AgreeToTerms)
            .Equal(true).WithMessage("Необхідно погодитись з умовами користування");
    }
}

public class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IEmailSender _email;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IPublishEndpoint _bus;
    private readonly IConfiguration _config;

    public RegisterUserCommandHandler(
        IUserRepository repo,
        IPasswordHasher hasher,
        IEmailSender email,
        IJwtTokenGenerator jwt,
        IPublishEndpoint bus,
        IConfiguration config)
    {
        _repo   = repo;
        _hasher = hasher;
        _email  = email;
        _jwt    = jwt;
        _bus    = bus;
        _config = config;
    }

    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand req, CancellationToken ct)
    {
        if (await _repo.ExistsByEmailAsync(req.Email, ct))
            return Result.Failure<RegisterUserResponse>("CONFLICT:Email вже зареєстровано");

        var baseUsername = UsernameGenerator.Build(req.FirstName, req.LastName);
        string username = "";
        for (var i = 0; i < 3; i++)
        {
            username = UsernameGenerator.WithSuffix(baseUsername);
            if (!await _repo.ExistsByUsernameAsync(username, ct)) break;
        }

        var passwordHash = _hasher.Hash(req.Password);
        var user = User.Create(req.Email, username, req.FirstName, req.LastName, passwordHash);

        var confirmToken = user.GenerateEmailConfirmationToken();

        await _repo.AddAsync(user, ct);
        await _repo.SaveChangesAsync(ct);

        var baseUrl = _config["App:BaseUrl"] ?? "http://localhost:3000";
        var confirmUrl = $"{baseUrl}/verify-email?token={Uri.EscapeDataString(confirmToken)}";
        await _email.SendEmailConfirmationAsync(user.Email, user.FirstName, confirmUrl, ct);

        await _bus.Publish(new UserRegisteredIntegrationEvent(
            user.Id, user.Email, user.Username, user.FirstName), ct);

        var jwtToken = _jwt.GenerateToken(user);

        return Result.Success(new RegisterUserResponse(
            user.Id,
            jwtToken,
            "Перевірте пошту для підтвердження акаунту"));
    }
}
