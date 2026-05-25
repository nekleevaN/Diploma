using FluentAssertions;
using TrustMarket.ChatService.Domain.Entities;
using Xunit;

namespace TrustMarket.ChatService.UnitTests.Domain;

public class MessageTests
{
    [Fact]
    public void Create_CleanMessage_IsCleanAndNotFlagged()
    {
        var msg = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "Привіт!", 0, null);

        msg.IsBlocked.Should().BeFalse();
        msg.IsFlagged.Should().BeFalse();
        msg.FraudScore.Should().Be(0);
    }

    [Fact]
    public void Create_SuspiciousScore_IsFlaggedButNotBlocked()
    {
        var msg = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "підозрілий текст", 50, "причина");

        msg.IsFlagged.Should().BeTrue();
        msg.IsBlocked.Should().BeFalse();
        msg.FraudReason.Should().Be("причина");
    }

    [Fact]
    public void Create_HighScore_IsBlocked()
    {
        var msg = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "шахрайство", 70, "card");

        msg.IsBlocked.Should().BeTrue();
        msg.IsFlagged.Should().BeFalse();
    }

    [Fact]
    public void Create_ScoreAt29_IsClean()
    {
        var msg = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "текст", 29, null);

        msg.IsBlocked.Should().BeFalse();
        msg.IsFlagged.Should().BeFalse();
    }

    [Fact]
    public void Create_ScoreAt30_IsFlagged()
    {
        var msg = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "текст", 30, "x");

        msg.IsFlagged.Should().BeTrue();
        msg.IsBlocked.Should().BeFalse();
    }
}
