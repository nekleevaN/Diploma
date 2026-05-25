using System.Text.RegularExpressions;

namespace TrustMarket.ChatService.Application.FraudDetection;

public class RuleBasedFraudAnalyzer : IFraudAnalyzer
{
    private static readonly Dictionary<string, int> ExternalPlatformKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        { "вайбер", 35 }, { "viber", 35 },
        { "телеграм", 45 }, { "telegram", 45 }, { "тг", 25 }, { "tg", 25 },
        { "вотсап", 40 }, { "whatsapp", 40 }, { "ватсап", 40 },
        { "сігнал", 30 }, { "signal", 30 },
        { "інстаграм", 25 }, { "instagram", 25 }, { "інста", 25 },
        { "фейсбук", 20 }, { "facebook", 20 }, { "messenger", 20 }
    };

    private static readonly (string Phrase, int Score)[] SuspiciousPhrases = new[]
    {
        ("перейдемо в", 40), ("перейдемо у", 40), ("перейдемо на", 40),
        ("давай в", 25), ("давай у", 25), ("давай на", 25),
        ("напиши на", 30), ("напишіть на", 30), ("пишіть на", 30),
        ("пишіть мені", 25), ("напиши мені", 25), ("напишіть мені", 25),
        ("мій номер", 35), ("мій телефон", 35),
        ("оплата на карту", 50), ("карта приват", 45), ("карта моно", 45),
        ("передоплата", 30), ("аванс", 25),
        ("без посередників", 35), ("напряму", 30),
        ("поза сайтом", 50), ("поза платформою", 50)
    };

    private static readonly Regex PhoneRegex = new(
        @"(\+?38)?\s?\(?0\d{2}\)?\s?\d{3}[\s-]?\d{2}[\s-]?\d{2}",
        RegexOptions.Compiled);

    private static readonly Regex CardRegex = new(
        @"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b",
        RegexOptions.Compiled);

    private static readonly Regex UrlRegex = new(
        @"https?://(?!trustmarket\.)[\w\.-]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public FraudAnalysisResult Analyze(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return new FraudAnalysisResult(0, null, new List<string>());

        var score = 0;
        var matches = new List<string>();
        var reasons = new List<string>();

        foreach (var (keyword, weight) in ExternalPlatformKeywords)
        {
            if (ContainsWord(message, keyword))
            {
                score += weight;
                matches.Add($"platform:{keyword}");
                reasons.Add($"згадка платформи '{keyword}'");
            }
        }

        foreach (var (phrase, weight) in SuspiciousPhrases)
        {
            if (message.Contains(phrase, StringComparison.OrdinalIgnoreCase))
            {
                score += weight;
                matches.Add($"phrase:{phrase}");
                reasons.Add($"фраза '{phrase}'");
            }
        }

        if (PhoneRegex.IsMatch(message))
        {
            score += 40;
            matches.Add("pattern:phone_number");
            reasons.Add("номер телефону в повідомленні");
        }

        if (CardRegex.IsMatch(message))
        {
            score += 70;
            matches.Add("pattern:card_number");
            reasons.Add("номер банківської карти");
        }

        if (UrlRegex.IsMatch(message))
        {
            score += 45;
            matches.Add("pattern:external_url");
            reasons.Add("посилання на зовнішній сайт");
        }

        score = Math.Min(score, 100);

        var reasonText = reasons.Count > 0
            ? "Виявлено: " + string.Join(", ", reasons)
            : null;

        return new FraudAnalysisResult(score, reasonText, matches);
    }

    private static bool ContainsWord(string text, string word)
    {
        var pattern = $@"\b{Regex.Escape(word)}\b";
        return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
    }
}
