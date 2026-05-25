using FluentValidation;
using MediatR;
using TrustMarket.Shared.Common.Results;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Domain.Repositories;

namespace TrustMarket.UserService.Application.Users.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<Result<LoginUserResponse>>;

public record LoginUserResponse(Guid UserId, string Token);

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);

        if (user is null || user.PasswordHash is null ||
            !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<LoginUserResponse>("Невірний email або пароль");

        if (!user.IsEmailConfirmed)
            return Result.Failure<LoginUserResponse>("EMAIL_NOT_CONFIRMED:Підтвердьте email перед входом");

        user.RecordLogin();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);

        var token = _tokenGenerator.GenerateToken(user);
        return Result.Success(new LoginUserResponse(user.Id, token));
    }
}
