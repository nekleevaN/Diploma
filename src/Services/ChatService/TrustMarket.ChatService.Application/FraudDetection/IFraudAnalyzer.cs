namespace TrustMarket.ChatService.Application.FraudDetection;

public interface IFraudAnalyzer
{
    FraudAnalysisResult Analyze(string message);
}

public record FraudAnalysisResult(
    int Score,
    string? Reason,
    List<string> Matches
)
{
    public bool IsClean => Score < 30;
    public bool IsSuspicious => Score >= 30 && Score < 70;
    public bool IsBlocked => Score >= 70;
}
