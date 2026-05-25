using MediatR;
using TrustMarket.Shared.Common.Results;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Application.Users.Helpers;
using TrustMarket.UserService.Domain.Entities;
using TrustMarket.UserService.Domain.Repositories;

namespace TrustMarket.UserService.Application.Users.Commands.GoogleAuth;

public record GoogleAuthCommand(string IdToken) : IRequest<Result<GoogleAuthResponse>>;

public record GoogleAuthResponse(Guid UserId, string JwtToken, bool IsNewUser);

public interface IGoogleTokenValidator
{
    Task<GooglePayload?> ValidateAsync(string idToken, CancellationToken ct = default);
}

public record GooglePayload(
    string Subject,
    string Email,
    string? FirstName,
    string? LastName);

public class GoogleAuthCommandHandler
    : IRequestHandler<GoogleAuthCommand, Result<GoogleAuthResponse>>
{
    private readonly IUserRepository _repo;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IGoogleTokenValidator _google;

    public GoogleAuthCommandHandler(
        IUserRepository repo, IJwtTokenGenerator jwt, IGoogleTokenValidator google)
    {
        _repo   = repo;
        _jwt    = jwt;
        _google = google;
    }

    public async Task<Result<GoogleAuthResponse>> Handle(
        GoogleAuthCommand req, CancellationToken ct)
    {
        var payload = await _google.ValidateAsync(req.IdToken, ct);
        if (payload is null)
            return Result.Failure<GoogleAuthResponse>("Недійсний Google токен");

        var user = await _repo.GetByEmailAsync(payload.Email, ct);

        if (user is not null)
        {
            if (user.AuthProvider == AuthProvider.Email)
                return Result.Failure<GoogleAuthResponse>(
                    "CONFLICT:Цей email зареєстрований через пароль. Увійдіть звичайним способом.");

            user.RecordLogin();
            _repo.Update(user);
            await _repo.SaveChangesAsync(ct);

            return Result.Success(new GoogleAuthResponse(user.Id, _jwt.GenerateToken(user), false));
        }

        var firstName = payload.FirstName ?? "Google";
        var lastName  = payload.LastName  ?? "User";

        var baseUsername = UsernameGenerator.Build(firstName, lastName);
        var username     = "";
        for (var i = 0; i < 3; i++)
        {
            username = UsernameGenerator.WithSuffix(baseUsername);
            if (!await _repo.ExistsByUsernameAsync(username, ct)) break;
        }

        var newUser = User.CreateWithGoogle(
            payload.Email, username, firstName, lastName, payload.Subject);

        await _repo.AddAsync(newUser, ct);
        await _repo.SaveChangesAsync(ct);

        return Result.Success(new GoogleAuthResponse(
            newUser.Id, _jwt.GenerateToken(newUser), true));
    }
}
