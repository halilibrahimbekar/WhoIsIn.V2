namespace WhoIsInV2.Domain.Entities;

public class EventParticipant
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid? UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ParticipantStatus Status { get; set; } = ParticipantStatus.Confirmed;
    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;

    public Event? Event { get; set; }
}
