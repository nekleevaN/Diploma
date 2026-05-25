using MediatR;
using TrustMarket.Shared.Common.Results;
using TrustMarket.UserService.Application.Abstractions;
using TrustMarket.UserService.Domain.Entities;
using TrustMarket.UserService.Domain.Repositories;

namespace TrustMarket.UserService.Application.Users.Commands.VerifyDiia;

public record StartDiiaVerificationCommand(Guid UserId) : IRequest<Result<StartDiiaVerificationResponse>>;
public record StartDiiaVerificationResponse(string SessionId);

public class StartDiiaVerificationCommandHandler
    : IRequestHandler<StartDiiaVerificationCommand, Result<StartDiiaVerificationResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IDiiaService _diiaService;

    public StartDiiaVerificationCommandHandler(IUserRepository userRepository, IDiiaService diiaService)
    {
        _userRepository = userRepository;
        _diiaService = diiaService;
    }

    public async Task<Result<StartDiiaVerificationResponse>> Handle(
        StartDiiaVerificationCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result.Failure<StartDiiaVerificationResponse>("Користувача не знайдено");

        if (user.HasBadge(BadgeType.DiiaVerified))
            return Result.Failure<StartDiiaVerificationResponse>("Вже верифіковано через Дію");

        var sessionId = await _diiaService.StartVerificationAsync(request.UserId, ct);
        return Result.Success(new StartDiiaVerificationResponse(sessionId));
    }
}

public record ConfirmDiiaVerificationCommand(Guid UserId, string SessionId) : IRequest<Result>;

public class ConfirmDiiaVerificationCommandHandler
    : IRequestHandler<ConfirmDiiaVerificationCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IDiiaService _diiaService;

    public ConfirmDiiaVerificationCommandHandler(IUserRepository userRepository, IDiiaService diiaService)
    {
        _userRepository = userRepository;
        _diiaService = diiaService;
    }

    public async Task<Result> Handle(ConfirmDiiaVerificationCommand request, CancellationToken ct)
    {
        var verificationResult = await _diiaService.VerifyAsync(request.SessionId, ct);
        if (verificationResult is null)
            return Result.Failure("Сесія не знайдена або вже використана");

        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result.Failure("Користувача не знайдено");

        if (user.HasBadge(BadgeType.DiiaVerified))
            return Result.Success();

        var badge = VerificationBadge.Create(user.Id, BadgeType.DiiaVerified);
        await _userRepository.AddBadgeAsync(badge, ct);

        return Result.Success();
    }
}
