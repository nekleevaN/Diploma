namespace TrustMarket.UserService.Infrastructure.Email;

internal static class EmailTemplates
{
    private static string Layout(string content) => $"""
        <!DOCTYPE html>
        <html lang="uk">
        <head>
          <meta charset="UTF-8"/>
          <meta name="viewport" content="width=device-width,initial-scale=1"/>
          <title>TrustMarket</title>
        </head>
        <body style="margin:0;padding:0;background:#f4ede4;font-family:Inter,Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0">
            <tr><td align="center" style="padding:32px 16px;">
              <table width="560" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.08);">
                <!-- Header -->
                <tr>
                  <td style="background:#708238;padding:24px 32px;">
                    <span style="font-size:22px;font-weight:700;color:#ffffff;letter-spacing:-.5px;">trustee</span>
                  </td>
                </tr>
                <!-- Body -->
                <tr>
                  <td style="padding:32px;">
                    {content}
                  </td>
                </tr>
                <!-- Footer -->
                <tr>
                  <td style="padding:20px 32px;border-top:1px solid #f0e8df;">
                    <p style="margin:0;font-size:12px;color:#9ca3af;">
                      © 2026 TrustMarket · Якщо виникли питання — напишіть нам
                    </p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;

    public static string ConfirmEmail(string firstName, string confirmationUrl) => Layout($"""
        <h1 style="margin:0 0 8px;font-size:22px;font-weight:700;color:#1a1a1a;">Привіт, {firstName}!</h1>
        <p style="margin:0 0 24px;font-size:15px;color:#4b5563;line-height:1.6;">
          Підтверди свою пошту на TrustMarket, щоб розпочати продавати і купувати.
        </p>
        <div style="text-align:center;margin:0 0 28px;">
          <a href="{confirmationUrl}"
             style="display:inline-block;padding:14px 32px;background:#708238;color:#ffffff;
                    font-size:15px;font-weight:600;text-decoration:none;border-radius:10px;">
            Підтвердити email
          </a>
        </div>
        <p style="margin:0 0 8px;font-size:13px;color:#6b7280;">Якщо кнопка не працює, скопіюй посилання:</p>
        <p style="margin:0 0 24px;font-size:12px;color:#708238;word-break:break-all;">
          <a href="{confirmationUrl}" style="color:#708238;">{confirmationUrl}</a>
        </p>
        <p style="margin:0;font-size:12px;color:#9ca3af;">
          Якщо ти не реєструвався на TrustMarket — просто проігноруй цей лист.
        </p>
        """);

    public static string ResetPassword(string firstName, string resetUrl) => Layout($"""
        <h1 style="margin:0 0 8px;font-size:22px;font-weight:700;color:#1a1a1a;">{firstName}, скидання паролю</h1>
        <p style="margin:0 0 24px;font-size:15px;color:#4b5563;line-height:1.6;">
          Ти (або хтось інший) надіслав запит на скидання паролю акаунту TrustMarket.
          Посилання дійсне протягом <strong>1 години</strong>.
        </p>
        <div style="text-align:center;margin:0 0 28px;">
          <a href="{resetUrl}"
             style="display:inline-block;padding:14px 32px;background:#708238;color:#ffffff;
                    font-size:15px;font-weight:600;text-decoration:none;border-radius:10px;">
            Скинути пароль
          </a>
        </div>
        <p style="margin:0 0 8px;font-size:13px;color:#6b7280;">Або скопіюй посилання вручну:</p>
        <p style="margin:0 0 24px;font-size:12px;color:#708238;word-break:break-all;">
          <a href="{resetUrl}" style="color:#708238;">{resetUrl}</a>
        </p>
        <p style="margin:0;font-size:12px;color:#9ca3af;">
          Якщо ти не запитував скидання паролю — просто проігноруй цей лист. Нічого не зміниться.
        </p>
        """);

    public static string Welcome(string firstName) => Layout($"""
        <h1 style="margin:0 0 8px;font-size:22px;font-weight:700;color:#1a1a1a;">
          Ласкаво просимо, {firstName}! 🎉
        </h1>
        <p style="margin:0 0 24px;font-size:15px;color:#4b5563;line-height:1.6;">
          Твій акаунт на TrustMarket підтверджено. Тепер ти можеш купувати і продавати!
        </p>
        <div style="display:flex;gap:12px;flex-wrap:wrap;margin:0 0 24px;">
          <div style="flex:1;min-width:150px;padding:16px;background:#f5f7ed;border-radius:10px;">
            <p style="margin:0 0 4px;font-size:13px;font-weight:600;color:#708238;">📋 Заповни профіль</p>
            <p style="margin:0;font-size:12px;color:#6b7280;">Додай фото і опис</p>
          </div>
          <div style="flex:1;min-width:150px;padding:16px;background:#f5f7ed;border-radius:10px;">
            <p style="margin:0 0 4px;font-size:13px;font-weight:600;color:#708238;">🔍 Огляньте каталог</p>
            <p style="margin:0;font-size:12px;color:#6b7280;">Тисячі товарів вже чекають</p>
          </div>
          <div style="flex:1;min-width:150px;padding:16px;background:#f5f7ed;border-radius:10px;">
            <p style="margin:0 0 4px;font-size:13px;font-weight:600;color:#708238;">🇺🇦 Дія-верифікація</p>
            <p style="margin:0;font-size:12px;color:#6b7280;">Підвищи рівень довіри</p>
          </div>
        </div>
        <p style="margin:0;font-size:13px;color:#9ca3af;">Дякуємо що обрав TrustMarket!</p>
        """);
}
