using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TrustMarket.UserService.Application.Users.Commands.ConfirmEmail;
using TrustMarket.UserService.Application.Users.Commands.ForgotPassword;
using TrustMarket.UserService.Application.Users.Commands.GoogleAuth;
using TrustMarket.UserService.Application.Users.Commands.LoginUser;
using TrustMarket.UserService.Application.Users.Commands.RegisterUser;
using TrustMarket.UserService.Application.Users.Commands.ResendVerification;
using TrustMarket.UserService.Application.Users.Commands.ResetPassword;
using TrustMarket.UserService.Application.Users.Commands.VerifyDiia;
using TrustMarket.UserService.Application.Users.Queries;

namespace TrustMarket.UserService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


    [HttpPost("register")]
    [EnableRateLimiting("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterUserCommand(
            req.FirstName, req.LastName, req.Email,
            req.Password, req.PasswordConfirm,
            req.AgreeToTerms, req.WantsNewsletter,
            req.Website, req.FormOpenedAt), ct);

        if (!result.IsSuccess)
        {
            if (result.Error!.StartsWith("CONFLICT:"))
                return Conflict(new { error = result.Error[9..] });
            return BadRequest(new { error = result.Error });
        }

        return Ok(new
        {
            userId  = result.Value!.UserId,
            token   = result.Value.Token,
            message = result.Value.Message
        });
    }


    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            if (result.Error!.StartsWith("EMAIL_NOT_CONFIRMED:"))
                return StatusCode(403, new
                {
                    code    = "EMAIL_NOT_VERIFIED",
                    message = result.Error[20..]
                });
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }


    [HttpGet("verify-email")]
    [EnableRateLimiting("verify-email")]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { error = "Токен відсутній" });

        var result = await _mediator.Send(new ConfirmEmailCommand(token), ct);

        if (!result.IsSuccess)
        {
            if (result.Error!.StartsWith("EXPIRED:"))
                return BadRequest(new { code = "TOKEN_EXPIRED", error = result.Error[8..] });
            return BadRequest(new { error = result.Error });
        }

        return Ok(new
        {
            userId      = result.Value!.UserId,
            token       = result.Value.JwtToken,
            redirectTo  = "/welcome"
        });
    }


    [HttpPost("resend-verification")]
    [Authorize]
    [EnableRateLimiting("resend")]
    public async Task<IActionResult> ResendVerification(CancellationToken ct)
    {
        var result = await _mediator.Send(new ResendVerificationCommand(CurrentUserId), ct);

        if (!result.IsSuccess)
        {
            if (result.Error!.StartsWith("RATE_LIMIT:"))
                return StatusCode(429, new { error = result.Error[11..] });
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Лист надіслано повторно" });
    }


    [HttpPost("forgot-password")]
    [EnableRateLimiting("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ForgotPasswordCommand(request.Email), ct);
        return Ok(new { message = "Якщо цей email зареєстровано, ви отримаєте лист" });
    }


    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ResetPasswordCommand(request.Token, request.NewPassword, request.ConfirmPassword), ct);

        if (!result.IsSuccess)
        {
            if (result.Error!.StartsWith("EXPIRED:"))
                return BadRequest(new { code = "TOKEN_EXPIRED", error = result.Error[8..] });
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { userId = result.Value!.UserId, token = result.Value.JwtToken });
    }


    [HttpGet("check-email")]
    public async Task<IActionResult> CheckEmail(
        [FromQuery] string email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Ok(new { available = true });

        var result = await _mediator.Send(new CheckEmailQuery(email), ct);
        return Ok(new { available = result.IsSuccess && result.Value });
    }


    [HttpPost("google")]
    public async Task<IActionResult> GoogleAuth(
        [FromBody] GoogleAuthRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GoogleAuthCommand(req.IdToken), ct);

        if (!result.IsSuccess)
        {
            if (result.Error!.StartsWith("CONFLICT:"))
                return Conflict(new { error = result.Error[9..] });
            return BadRequest(new { error = result.Error });
        }

        return Ok(new
        {
            userId    = result.Value!.UserId,
            token     = result.Value.JwtToken,
            isNewUser = result.Value.IsNewUser
        });
    }
}

public record GoogleAuthRequest(string IdToken);

public class RegisterRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string PasswordConfirm { get; set; } = "";
    public bool AgreeToTerms { get; set; }
    public bool WantsNewsletter { get; set; }
    public string? Website { get; set; }
    public long? FormOpenedAt { get; set; }
}

public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword, string ConfirmPassword);

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpPost("verify/diia/start")]
    [Authorize]
    public async Task<IActionResult> StartDiiaVerification(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new StartDiiaVerificationCommand(userId), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("verify/diia/confirm")]
    [Authorize]
    public async Task<IActionResult> ConfirmDiiaVerification([FromBody] ConfirmDiiaRequest request, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new ConfirmDiiaVerificationCommand(userId, request.SessionId), ct);
        return result.IsSuccess
            ? Ok(new { message = "Верифікацію через Дію успішно пройдено" })
            : BadRequest(new { error = result.Error });
    }
}

public record ConfirmDiiaRequest(string SessionId);
