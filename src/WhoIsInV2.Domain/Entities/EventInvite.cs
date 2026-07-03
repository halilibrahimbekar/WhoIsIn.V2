namespace WhoIsInV2.Domain.Entities;

public class EventInvite
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string InviteToken { get; set; } = string.Empty;
    public InviteStatus Status { get; set; } = InviteStatus.Pending;
    public DateTime InvitedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAtUtc { get; set; }

    public Event? Event { get; set; }
}
