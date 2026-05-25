using FluentAssertions;
using TrustMarket.ChatService.Domain.Entities;
using Xunit;

namespace TrustMarket.ChatService.UnitTests.Domain;

public class ViewingRequestTests
{
    private static ViewingRequest Make(Guid? proposerId = null, Guid? responderId = null) =>
        ViewingRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            proposerId ?? Guid.NewGuid(), responderId ?? Guid.NewGuid(),
            "Телефон Samsung", "Київ, Хрещатик",
            DateTime.UtcNow.AddDays(1));

    [Fact]
    public void Create_SetsPendingStatusAndFields()
    {
        var proposerId = Guid.NewGuid();
        var vr = Make(proposerId: proposerId);

        vr.Status.Should().Be(ViewingStatus.Pending);
        vr.ProposerId.Should().Be(proposerId);
        vr.FollowUpSent.Should().BeFalse();
    }

    [Fact]
    public void Accept_SetsAcceptedStatus()
    {
        var vr = Make();

        vr.Accept();

        vr.Status.Should().Be(ViewingStatus.Accepted);
    }

    [Fact]
    public void Decline_SetsDeclinedStatus()
    {
        var vr = Make();

        vr.Decline();

        vr.Status.Should().Be(ViewingStatus.Declined);
    }

    [Fact]
    public void Reschedule_SwapsRolesAndSetsPending()
    {
        var originalProposer = Guid.NewGuid();
        var originalResponder = Guid.NewGuid();
        var vr = Make(proposerId: originalProposer, responderId: originalResponder);
        var newDate = DateTime.UtcNow.AddDays(2);

        vr.Reschedule(newDate);

        vr.Status.Should().Be(ViewingStatus.Pending);
        vr.ProposedDateTime.Should().Be(newDate);
        // Roles swapped: original responder is now proposer
        vr.ProposerId.Should().Be(originalResponder);
        vr.ResponderId.Should().Be(originalProposer);
    }

    [Fact]
    public void MarkFollowUpSent_SetsFlag()
    {
        var vr = Make();

        vr.MarkFollowUpSent();

        vr.FollowUpSent.Should().BeTrue();
    }

    [Fact]
    public void SetFollowUpAction_StoresAction()
    {
        var vr = Make();

        vr.SetFollowUpAction("buy");

        vr.FollowUpAction.Should().Be("buy");
    }

    [Fact]
    public void SetResponderTrustedContact_StoresBothContacts()
    {
        var vr = Make();

        vr.SetResponderTrustedContact(123456789L, "test@mail.com");

        vr.ResponderTrustedTelegramId.Should().Be(123456789L);
        vr.ResponderTrustedEmail.Should().Be("test@mail.com");
    }
}
