using FluentAssertions;
using TrustMarket.ChatService.Application.FraudDetection;
using Xunit;

namespace TrustMarket.ChatService.UnitTests.FraudDetection;

public class RuleBasedFraudAnalyzerTests
{
    private readonly RuleBasedFraudAnalyzer _analyzer = new();

    [Fact]
    public void Analyze_CleanMessage_ReturnsLowScore()
    {
        var result = _analyzer.Analyze("Доброго дня! Чи актуальне ще оголошення?");

        result.IsClean.Should().BeTrue();
        result.Score.Should().BeLessThan(30);
    }

    [Theory]
    [InlineData("Давайте перейдемо в вайбер для зручності")]
    [InlineData("Напишіть мені в телеграм")]
    [InlineData("Краще пишіть на whatsapp")]
    public void Analyze_ExternalPlatformMention_ReturnsBlocked(string message)
    {
        var result = _analyzer.Analyze(message);

        result.IsBlocked.Should().BeTrue();
        result.Score.Should().BeGreaterOrEqualTo(70);
        result.Reason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Analyze_PhoneNumber_ReturnsHighScore()
    {
        var result = _analyzer.Analyze("Мій номер +380501234567, дзвоніть");

        result.Score.Should().BeGreaterOrEqualTo(70);
        result.Matches.Should().Contain(m => m.Contains("phone"));
    }

    [Fact]
    public void Analyze_CardNumber_ReturnsBlocked()
    {
        var result = _analyzer.Analyze("Переведіть на карту 4441 1144 5555 6666");

        result.IsBlocked.Should().BeTrue();
        result.Matches.Should().Contain(m => m.Contains("card"));
    }

    [Fact]
    public void Analyze_ExternalUrl_ReturnsHighScore()
    {
        var result = _analyzer.Analyze("Ось посилання на оплату https://fake-payment.com/pay");

        result.Score.Should().BeGreaterOrEqualTo(45);
        result.Matches.Should().Contain(m => m.Contains("url"));
    }

    [Fact]
    public void Analyze_EmptyMessage_ReturnsZero()
    {
        var result = _analyzer.Analyze("");

        result.Score.Should().Be(0);
        result.IsClean.Should().BeTrue();
    }

    [Fact]
    public void Analyze_PartialWordMatch_DoesNotTrigger()
    {
        // "тг" не повинен матчитися всередині слова "хтось"
        var result = _analyzer.Analyze("Хтось купив це вже?");

        result.IsClean.Should().BeTrue();
    }

    [Fact]
    public void Analyze_MultipleTriggers_AccumulatesScore()
    {
        var result = _analyzer.Analyze(
            "Перейдемо в вайбер, мій номер +380671234567, оплата на карту");

        result.IsBlocked.Should().BeTrue();
        result.Matches.Count.Should().BeGreaterOrEqualTo(3);
    }
}
