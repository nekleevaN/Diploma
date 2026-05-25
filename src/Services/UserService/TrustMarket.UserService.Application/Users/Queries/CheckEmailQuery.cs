using MediatR;
using TrustMarket.Shared.Common.Results;
using TrustMarket.UserService.Domain.Repositories;

namespace TrustMarket.UserService.Application.Users.Queries;

public record CheckEmailQuery(string Email) : IRequest<Result<bool>>;

public class CheckEmailQueryHandler : IRequestHandler<CheckEmailQuery, Result<bool>>
{
    private readonly IUserRepository _repo;

    public CheckEmailQueryHandler(IUserRepository repo) => _repo = repo;

    public async Task<Result<bool>> Handle(CheckEmailQuery req, CancellationToken ct)
    {
        var exists = await _repo.ExistsByEmailAsync(req.Email, ct);
        return Result.Success(!exists);
    }
}
