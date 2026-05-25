using MassTransit;
using MediatR;
using TrustMarket.Shared.Common.Results;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Domain.Repositories;

namespace TrustMarket.UserService.Application.Users.Commands.ConfirmEmail;

public record ConfirmEmailCommand(string Token) : IRequest<Result<ConfirmEmailResponse>>;

public record ConfirmEmailResponse(Guid UserId, string JwtToken);

public class ConfirmEmailCommandHandler
    : IRequestHandler<ConfirmEmailCommand, Result<ConfirmEmailResponse>>
{
    private readonly IUserRepository _repo;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IEmailSender _email;
    private readonly IPublishEndpoint _bus;

    public ConfirmEmailCommandHandler(
        IUserRepository repo, IJwtTokenGenerator jwt,
        IEmailSender email, IPublishEndpoint bus)
    {
        _repo  = repo;
        _jwt   = jwt;
        _email = email;
        _bus   = bus;
    }

    public async Task<Result<ConfirmEmailResponse>> Handle(
        ConfirmEmailCommand req, CancellationToken ct)
    {
        var user = await _repo.GetByEmailConfirmationTokenAsync(req.Token, ct);

        if (user is null)
            return Result.Failure<ConfirmEmailResponse>("Посилання недійсне або вже використане");

        if (user.EmailConfirmationTokenExpiresAt < DateTime.UtcNow)
            return Result.Failure<ConfirmEmailResponse>(
                "EXPIRED:Термін дії посилання вичерпано. Запросіть нове.");

        var (ok, newBadge) = user.TryConfirmEmail(req.Token);
        if (!ok)
            return Result.Failure<ConfirmEmailResponse>("Посилання недійсне");

        if (newBadge is not null)
            _repo.TrackBadge(newBadge);

        await _repo.SaveChangesAsync(ct);

        await _email.SendWelcomeAsync(user.Email, user.FirstName, ct);

        await _bus.Publish(
            new UserEmailConfirmedIntegrationEvent(user.Id, user.Email, user.FirstName), ct);

        var token = _jwt.GenerateToken(user);
        return Result.Success(new ConfirmEmailResponse(user.Id, token));
    }
}
