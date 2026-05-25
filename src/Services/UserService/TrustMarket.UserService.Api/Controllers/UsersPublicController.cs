using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrustMarket.Shared.Contracts.IntegrationEvents;
using TrustMarket.UserService.Domain.Repositories;

namespace TrustMarket.UserService.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersPublicController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public UsersPublicController(IUserRepository userRepository, IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPublicProfile(Guid id, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user is null) return NotFound();

        return Ok(new
        {
            userId = user.Id,
            username = user.Username,
            displayName = user.DisplayName,
            firstName = user.FirstName,
            lastName = user.LastName,
            avatarUrl = user.AvatarUrl,
            bio = user.Bio,
            rating = user.Rating,
            sellerRating = user.SellerRating,
            sellerReviewCount = user.SellerReviewCount,
            buyerRating = user.BuyerRating,
            buyerReviewCount = user.BuyerReviewCount,
            badges = user.Badges.Select(b => b.Type.ToString()),
            joinedAt = user.CreatedAt,
            trustedContactTelegramId = user.TrustedContactTelegramId,
            trustedContactEmail = user.TrustedContactEmail,
            monobankSubMerchantId = user.MonobankSubMerchantId,
            isPayoutEnabled = !string.IsNullOrEmpty(user.MonobankSubMerchantId)
        });
    }

    [HttpPost("avatar")]
    [Authorize]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Файл не надано" });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { error = "Файл перевищує 5 МБ" });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null) return NotFound();

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
        Directory.CreateDirectory(uploadsPath);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var filename = $"{userId}{ext}";
        var filePath = Path.Combine(uploadsPath, filename);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream, ct);

        var url = $"/uploads/avatars/{filename}";
        user.UpdateAvatar(url);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);

        return Ok(new { avatarUrl = url });
    }

    [HttpPost("me/test-trusted-email")]
    [Authorize]
    public async Task<IActionResult> TestTrustedEmail(
        [FromServices] TrustMarket.UserService.Application.Abstractions.IEmailSender emailSender,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null) return NotFound();

        var email = user.TrustedContactEmail;
        if (string.IsNullOrEmpty(email))
            return BadRequest(new { error = "TrustedContactEmail не вказано. Збережіть email довіреної особи в профілі." });

        var buyer  = user.DisplayName;
        var seller = "Марія К. (продавець)";
        var dt     = DateTime.Now.AddHours(2);
        var dateStr = $"{dt:dddd, dd MMMM yyyy} о {dt:HH:mm}";
        const string adTitle  = "Куртка зимова Nike (тест-оголошення)";
        const string location = "Київ, вул. Хрещатик 1";

        await emailSender.SendRawAsync(email,
            $"🛡️ {buyer} іде на перегляд — {dt:dd.MM HH:mm}",
            $"""
            <!DOCTYPE html>
            <html lang="uk"><head><meta charset="UTF-8"/></head>
            <body style="margin:0;padding:0;background:#f4ede4;font-family:Inter,Arial,sans-serif">
            <table width="100%" cellpadding="0" cellspacing="0">
              <tr><td align="center" style="padding:32px 16px">
                <table width="560" cellpadding="0" cellspacing="0" style="background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.08)">
                  <tr><td style="background:#708238;padding:20px 32px">
                    <span style="font-size:22px;font-weight:700;color:#fff;letter-spacing:-.5px">trustee</span>
                  </td></tr>
                  <tr><td style="padding:28px 32px 16px">
                    <p style="margin:0 0 4px;font-size:12px;color:#9ca3af;text-transform:uppercase">Сповіщення безпеки</p>
                    <h1 style="margin:0 0 20px;font-size:22px;color:#1a1a1a;line-height:1.3">
                      <strong>{buyer}</strong> іде на перегляд до <strong>{seller}</strong>
                    </h1>
                  </td></tr>
                  <tr><td style="padding:0 32px 24px">
                    <table width="100%" cellpadding="0" cellspacing="0" style="border:1px solid #f0e8df;border-radius:12px;overflow:hidden">
                      <tr><td style="padding:12px 16px;border-bottom:1px solid #f0e8df">
                        <span style="color:#9ca3af;font-size:13px">👤 Покупець</span><br>
                        <strong style="color:#1a1a1a">{buyer}</strong>
                      </td></tr>
                      <tr><td style="padding:12px 16px;border-bottom:1px solid #f0e8df">
                        <span style="color:#9ca3af;font-size:13px">🏷️ Продавець</span><br>
                        <strong style="color:#1a1a1a">{seller}</strong>
                      </td></tr>
                      <tr><td style="padding:12px 16px;border-bottom:1px solid #f0e8df">
                        <span style="color:#9ca3af;font-size:13px">📦 Товар</span><br>
                        <strong style="color:#1a1a1a">{adTitle}</strong>
                      </td></tr>
                      <tr><td style="padding:12px 16px;border-bottom:1px solid #f0e8df">
                        <span style="color:#9ca3af;font-size:13px">🕐 Час</span><br>
                        <strong style="color:#708238;font-size:16px">{dateStr}</strong>
                      </td></tr>
                      <tr><td style="padding:12px 16px">
                        <span style="color:#9ca3af;font-size:13px">📍 Місце</span><br>
                        <strong style="color:#1a1a1a">{location}</strong>
                      </td></tr>
                    </table>
                  </td></tr>
                  <tr><td style="padding:0 32px 28px">
                    <div style="background:#fef3c7;border-radius:10px;padding:14px 16px">
                      <p style="margin:0;font-size:13px;color:#92400e">
                        ⚠️ Якщо <strong>{buyer}</strong> не повернеться вчасно — зв'яжіться з ним або зверніться до поліції.
                      </p>
                    </div>
                  </td></tr>
                  <tr><td style="padding:16px 32px;border-top:1px solid #f0e8df">
                    <p style="margin:0;font-size:12px;color:#9ca3af">© 2026 TrustMarket · Автоматичне сповіщення безпеки</p>
                  </td></tr>
                </table>
              </td></tr>
            </table>
            </body></html>
            """, ct);

        return Ok(new { message = $"Тестовий лист надіслано на {email}" });
    }

    [HttpPost("me/test-trusted-telegram")]
    [Authorize]
    public async Task<IActionResult> TestTrustedTelegram(
        [FromServices] MassTransit.IPublishEndpoint bus,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user   = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null) return NotFound();

        var telegramId = user.TrustedContactTelegramId;
        if (telegramId is null or 0)
            return BadRequest(new { error = "TrustedContactTelegramId не вказано. Збережіть Telegram ID в профілі." });

        await bus.Publish(new TrustMarket.Shared.Contracts.IntegrationEvents.ViewingConfirmedIntegrationEvent(
            ViewingId:              Guid.NewGuid(),
            ChatId:                 Guid.NewGuid(),
            BuyerId:                user.Id,
            SellerId:               Guid.NewGuid(),
            AdTitle:                "Куртка зимова Nike (тест)",
            BuyerName:              user.DisplayName,
            SellerName:             "Марія К.",
            ViewingDateTime:        DateTime.UtcNow.AddHours(2),
            LocationAddress:        "Київ, вул. Хрещатик 1",
            BuyerTrustedTelegramId: telegramId,
            SellerTrustedTelegramId: null,
            BuyerTrustedEmail:      null,
            SellerTrustedEmail:     null), ct);

        return Ok(new { message = $"Подію опубліковано → Telegram ID {telegramId} отримає повідомлення" });
    }

    [HttpPut("me/profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        [FromServices] MassTransit.IPublishEndpoint bus,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null) return NotFound();

        user.UpdateProfile(request.Bio, request.TrustedContactTelegramId, request.TrustedContactEmail);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);

        await bus.Publish(
            new TrustMarket.Shared.Contracts.IntegrationEvents.UserProfileUpdatedIntegrationEvent(
                user.Id, user.DisplayName, user.FullName), ct);

        return Ok(new
        {
            bio = user.Bio,
            trustedContactTelegramId = user.TrustedContactTelegramId,
            trustedContactEmail = user.TrustedContactEmail,
            monobankSubMerchantId = user.MonobankSubMerchantId
        });
    }

    [HttpPut("me/payout")]
    [Authorize]
    public async Task<IActionResult> SetPayoutMethod(
        [FromBody] SetPayoutRequest request,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user is null) return NotFound();

        user.SetMonobankSubMerchantId(request.MonobankSubMerchantId);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(
            new UserPayoutMethodUpdatedIntegrationEvent(userId, user.MonobankSubMerchantId), ct);

        var isSet = !string.IsNullOrEmpty(user.MonobankSubMerchantId);
        return Ok(new
        {
            monobankSubMerchantId = user.MonobankSubMerchantId,
            payoutEnabled = isSet,
            message = isSet
                ? "Автоматичні виплати увімкнено. Кошти надходитимуть на ваш Monobank-рахунок після завершення угоди."
                : "SubMerchant ID видалено. Виплати здійснюватимуться вручну."
        });
    }
}

public record SetPayoutRequest(string? MonobankSubMerchantId);

public record UpdateProfileRequest(
    string? Bio,
    long? TrustedContactTelegramId = null,
    string? TrustedContactEmail = null);

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string folder = "general", CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "Файл не надано" });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { error = "Файл перевищує 10 МБ" });

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folder);
        Directory.CreateDirectory(uploadsPath);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var filename = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsPath, filename);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream, ct);

        return Ok(new { url = $"/uploads/{folder}/{filename}" });
    }
}
