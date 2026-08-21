namespace WhoIsInV2.Domain.Entities;

public class Event
{
    public Guid Id { get; set; }
    public Guid OrganizerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public EventVisibility Visibility { get; set; } = EventVisibility.InviteOnly;
    public bool RequireApproval { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime? EndAtUtc { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string? LocationName { get; set; }
    public string? LocationAddress { get; set; }
    public string? OnlineMeetingUrl { get; set; }
    public int Capacity { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Draft;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public User? Organizer { get; set; }
    public Category? Category { get; set; }
    public ICollection<EventInvite> Invites { get; set; } = [];
    public ICollection<EventParticipant> Participants { get; set; } = [];
}
