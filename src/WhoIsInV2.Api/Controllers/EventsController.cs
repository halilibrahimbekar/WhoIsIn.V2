using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhoIsInV2.Domain.Entities;
using WhoIsInV2.Infrastructure.Persistence;

namespace WhoIsInV2.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly WhoIsInV2DbContext _dbContext;

    public EventsController(WhoIsInV2DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<EventListItemResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var events = await _dbContext.Events
            .AsNoTracking()
            .OrderBy(x => x.StartAtUtc)
            .Select(x => new EventListItemResponse(
                x.Id,
                x.Title,
                x.StartAtUtc,
                x.EndAtUtc,
                x.Capacity,
                x.Status.ToString()))
            .ToListAsync(cancellationToken);

        return Ok(events);
    }

    [Authorize]
    [HttpGet("summary")]
    public async Task<ActionResult<EventSummaryResponse>> GetSummary(CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var events = await _dbContext.Events
            .AsNoTracking()
            .Where(x => x.OrganizerId == currentUserId.Value)
            .OrderBy(x => x.StartAtUtc)
            .Select(x => new EventSummaryItemResponse(
                x.Id,
                x.Title,
                x.StartAtUtc,
                x.EndAtUtc,
                x.LocationName,
                x.OnlineMeetingUrl,
                x.Capacity,
                x.Status.ToString(),
                x.Participants.Count(p => p.Status == ParticipantStatus.Confirmed),
                x.Participants.Count(p => p.Status == ParticipantStatus.Waitlisted)))
            .ToListAsync(cancellationToken);

        var acceptedGuests = events.Sum(x => x.AcceptedCount);
        var waitlistedGuests = events.Sum(x => x.WaitlistCount);
        var totalCapacity = events.Sum(x => x.Capacity);

        return Ok(new EventSummaryResponse(
            events.Count(x => x.Status is nameof(EventStatus.Published) or nameof(EventStatus.Draft)),
            acceptedGuests,
            waitlistedGuests,
            totalCapacity == 0 ? 0 : Math.Round(acceptedGuests * 100d / totalCapacity, 1),
            events.Take(5).ToArray()));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _dbContext.Events
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new EventDetailResponse(
                x.Id,
                x.OrganizerId,
                x.Title,
                x.Description,
                x.Category,
                x.StartAtUtc,
                x.EndAtUtc,
                x.TimeZone,
                x.LocationName,
                x.LocationAddress,
                x.OnlineMeetingUrl,
                x.Capacity,
                x.Status.ToString()))
            .SingleOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<EventDetailResponse>> Create([FromBody] CreateEventRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        if (request.Capacity <= 0)
        {
            return BadRequest("Capacity must be greater than zero.");
        }

        if (request.EndAtUtc is not null && request.EndAtUtc <= request.StartAtUtc)
        {
            return BadRequest("EndAtUtc must be greater than StartAtUtc.");
        }

        var organizerExists = await _dbContext.Users
            .AnyAsync(x => x.Id == currentUserId.Value, cancellationToken);

        if (!organizerExists)
        {
            return Unauthorized();
        }

        var entity = new Event
        {
            Id = Guid.NewGuid(),
            OrganizerId = currentUserId.Value,
            Title = request.Title.Trim(),
            Description = request.Description,
            Category = request.Category,
            StartAtUtc = request.StartAtUtc,
            EndAtUtc = request.EndAtUtc,
            TimeZone = request.TimeZone,
            LocationName = request.LocationName,
            LocationAddress = request.LocationAddress,
            OnlineMeetingUrl = request.OnlineMeetingUrl,
            Capacity = request.Capacity,
            Status = EventStatus.Draft,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Events.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new EventDetailResponse(
            entity.Id,
            entity.OrganizerId,
            entity.Title,
            entity.Description,
            entity.Category,
            entity.StartAtUtc,
            entity.EndAtUtc,
            entity.TimeZone,
            entity.LocationName,
            entity.LocationAddress,
            entity.OnlineMeetingUrl,
            entity.Capacity,
            entity.Status.ToString());

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EventDetailResponse>> Update(Guid id, [FromBody] UpdateEventRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Title is required.");
        }

        if (request.Capacity <= 0)
        {
            return BadRequest("Capacity must be greater than zero.");
        }

        if (request.EndAtUtc is not null && request.EndAtUtc <= request.StartAtUtc)
        {
            return BadRequest("EndAtUtc must be greater than StartAtUtc.");
        }

        var entity = await _dbContext.Events
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.OrganizerId != currentUserId.Value)
        {
            return Forbid();
        }

        entity.Title = request.Title.Trim();
        entity.Description = request.Description;
        entity.Category = request.Category;
        entity.StartAtUtc = request.StartAtUtc;
        entity.EndAtUtc = request.EndAtUtc;
        entity.TimeZone = request.TimeZone;
        entity.LocationName = request.LocationName;
        entity.LocationAddress = request.LocationAddress;
        entity.OnlineMeetingUrl = request.OnlineMeetingUrl;
        entity.Capacity = request.Capacity;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToEventDetailResponse(entity));
    }

    [Authorize]
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateEventStatusRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var entity = await _dbContext.Events.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.OrganizerId != currentUserId.Value)
        {
            return Forbid();
        }

        if (!Enum.TryParse<EventStatus>(request.Status, ignoreCase: true, out var nextStatus))
        {
            return BadRequest("Invalid status value.");
        }

        if (!IsValidStatusTransition(entity.Status, nextStatus))
        {
            return BadRequest($"Cannot change event status from {entity.Status} to {nextStatus}.");
        }

        entity.Status = nextStatus;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet("{id:guid}/invites")]
    public async Task<ActionResult<IReadOnlyCollection<EventInviteResponse>>> GetInvites(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var ownsEvent = await _dbContext.Events
            .AnyAsync(x => x.Id == id && x.OrganizerId == currentUserId.Value, cancellationToken);

        if (!ownsEvent)
        {
            var exists = await _dbContext.Events.AnyAsync(x => x.Id == id, cancellationToken);
            return exists ? Forbid() : NotFound();
        }

        var invites = await _dbContext.EventInvites
            .AsNoTracking()
            .Where(x => x.EventId == id)
            .OrderBy(x => x.Email)
            .Select(x => new EventInviteResponse(x.Id, x.Email, x.Status.ToString(), x.InvitedAtUtc, x.RespondedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(invites);
    }

    [Authorize]
    [HttpPost("{id:guid}/invites")]
    public async Task<ActionResult<IReadOnlyCollection<EventInviteResponse>>> Invite(Guid id, [FromBody] InviteUsersRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var entity = await _dbContext.Events
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.OrganizerId != currentUserId)
        {
            return Forbid();
        }

        var normalizedEmails = request.Emails
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedEmails.Length == 0)
        {
            return BadRequest("At least one valid email is required.");
        }

        var existing = await _dbContext.EventInvites
            .Where(x => x.EventId == id && normalizedEmails.Contains(x.Email))
            .ToListAsync(cancellationToken);

        var existingEmailSet = existing
            .Select(x => x.Email)
            .ToHashSet(StringComparer.Ordinal);

        var created = new List<EventInvite>();
        foreach (var email in normalizedEmails)
        {
            if (existingEmailSet.Contains(email))
            {
                continue;
            }

            created.Add(new EventInvite
            {
                Id = Guid.NewGuid(),
                EventId = id,
                Email = email,
                InviteToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
                Status = InviteStatus.Pending,
                InvitedAtUtc = DateTime.UtcNow
            });
        }

        if (created.Count > 0)
        {
            _dbContext.EventInvites.AddRange(created);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var response = created
            .OrderBy(x => x.Email)
            .Select(x => new EventInviteResponse(x.Id, x.Email, x.Status.ToString(), x.InvitedAtUtc, x.RespondedAtUtc))
            .ToList();

        return Ok(response);
    }

    [Authorize]
    [HttpPost("{id:guid}/rsvp")]
    public async Task<ActionResult<RsvpResponse>> Rsvp(Guid id, [FromBody] RsvpRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == currentUserId.Value, cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        var entity = await _dbContext.Events
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        var invite = await _dbContext.EventInvites
            .SingleOrDefaultAsync(x => x.EventId == id && x.Email == user.Email, cancellationToken);

        if (invite is null)
        {
            return BadRequest("No invite found for this user and event.");
        }

        if (!Enum.TryParse<InviteStatus>(request.Decision, ignoreCase: true, out var decision)
            || (decision != InviteStatus.Accepted && decision != InviteStatus.Declined))
        {
            return BadRequest("Decision must be Accepted or Declined.");
        }

        var participant = await _dbContext.EventParticipants
            .SingleOrDefaultAsync(x => x.EventId == id && x.Email == user.Email, cancellationToken);

        if (decision == InviteStatus.Declined)
        {
            invite.Status = InviteStatus.Declined;
            invite.RespondedAtUtc = DateTime.UtcNow;

            if (participant is null)
            {
                _dbContext.EventParticipants.Add(new EventParticipant
                {
                    Id = Guid.NewGuid(),
                    EventId = id,
                    UserId = user.Id,
                    DisplayName = $"{user.FirstName} {user.LastName}".Trim(),
                    Email = user.Email,
                    Status = ParticipantStatus.Declined,
                    AddedAtUtc = DateTime.UtcNow
                });
            }
            else
            {
                participant.Status = ParticipantStatus.Declined;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await TryPromoteWaitlistedParticipant(id, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new RsvpResponse(id, user.Email, invite.Status.ToString(), ParticipantStatus.Declined.ToString()));
        }

        var confirmedCount = await _dbContext.EventParticipants
            .CountAsync(x => x.EventId == id && x.Status == ParticipantStatus.Confirmed, cancellationToken);

        var participantStatus = confirmedCount < entity.Capacity
            ? ParticipantStatus.Confirmed
            : ParticipantStatus.Waitlisted;

        invite.Status = participantStatus == ParticipantStatus.Confirmed
            ? InviteStatus.Accepted
            : InviteStatus.Waitlisted;
        invite.RespondedAtUtc = DateTime.UtcNow;

        if (participant is null)
        {
            _dbContext.EventParticipants.Add(new EventParticipant
            {
                Id = Guid.NewGuid(),
                EventId = id,
                UserId = user.Id,
                DisplayName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email,
                Status = participantStatus,
                AddedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            participant.Status = participantStatus;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new RsvpResponse(id, user.Email, invite.Status.ToString(), participantStatus.ToString()));
    }

    [Authorize]
    [HttpGet("{id:guid}/participants")]
    public async Task<ActionResult<IReadOnlyCollection<EventParticipantResponse>>> GetParticipants(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var entity = await _dbContext.Events
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.OrganizerId != currentUserId.Value)
        {
            return Forbid();
        }

        var participants = await _dbContext.EventParticipants
            .AsNoTracking()
            .Where(x => x.EventId == id)
            .OrderBy(x => x.AddedAtUtc)
            .Select(x => new EventParticipantResponse(x.Id, x.Email, x.DisplayName, x.Status.ToString(), x.AddedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(participants);
    }

    [Authorize]
    [HttpPatch("{id:guid}/participants/{participantId:guid}")]
    public async Task<IActionResult> UpdateParticipantStatus(
        Guid id,
        Guid participantId,
        [FromBody] UpdateParticipantStatusRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var entity = await _dbContext.Events
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.OrganizerId != currentUserId.Value)
        {
            return Forbid();
        }

        if (!Enum.TryParse<ParticipantStatus>(request.Status, ignoreCase: true, out var nextStatus))
        {
            return BadRequest("Invalid participant status.");
        }

        var participant = await _dbContext.EventParticipants
            .SingleOrDefaultAsync(x => x.Id == participantId && x.EventId == id, cancellationToken);

        if (participant is null)
        {
            return NotFound();
        }

        if (nextStatus == ParticipantStatus.Confirmed && participant.Status != ParticipantStatus.Confirmed)
        {
            var confirmedCount = await _dbContext.EventParticipants
                .CountAsync(x => x.EventId == id && x.Status == ParticipantStatus.Confirmed, cancellationToken);

            if (confirmedCount >= entity.Capacity)
            {
                return Conflict("Event capacity has been reached.");
            }
        }

        participant.Status = nextStatus;

        var invite = await _dbContext.EventInvites
            .SingleOrDefaultAsync(x => x.EventId == id && x.Email == participant.Email, cancellationToken);

        if (invite is not null && nextStatus is ParticipantStatus.Confirmed or ParticipantStatus.Waitlisted or ParticipantStatus.Declined)
        {
            invite.Status = nextStatus switch
            {
                ParticipantStatus.Confirmed => InviteStatus.Accepted,
                ParticipantStatus.Waitlisted => InviteStatus.Waitlisted,
                _ => InviteStatus.Declined
            };
            invite.RespondedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/waitlist/promote")]
    public async Task<IActionResult> PromoteWaitlistedParticipant(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
        {
            return Unauthorized();
        }

        var entity = await _dbContext.Events
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        if (entity.OrganizerId != currentUserId.Value)
        {
            return Forbid();
        }

        var confirmedCount = await _dbContext.EventParticipants
            .CountAsync(x => x.EventId == id && x.Status == ParticipantStatus.Confirmed, cancellationToken);

        if (confirmedCount >= entity.Capacity)
        {
            return Conflict("Event capacity has been reached.");
        }

        var waitlisted = await _dbContext.EventParticipants
            .Where(x => x.EventId == id && x.Status == ParticipantStatus.Waitlisted)
            .OrderBy(x => x.AddedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (waitlisted is null)
        {
            return NotFound("No waitlisted participant found.");
        }

        waitlisted.Status = ParticipantStatus.Confirmed;

        var invite = await _dbContext.EventInvites
            .FirstOrDefaultAsync(x => x.EventId == id && x.Email == waitlisted.Email, cancellationToken);

        if (invite is not null)
        {
            invite.Status = InviteStatus.Accepted;
            invite.RespondedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var userId) ? userId : null;
    }

    private static bool IsValidStatusTransition(EventStatus currentStatus, EventStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return true;
        }

        return currentStatus switch
        {
            EventStatus.Draft => nextStatus is EventStatus.Published or EventStatus.Cancelled,
            EventStatus.Published => nextStatus is EventStatus.Cancelled or EventStatus.Completed,
            _ => false
        };
    }

    private static EventDetailResponse ToEventDetailResponse(Event entity)
    {
        return new EventDetailResponse(
            entity.Id,
            entity.OrganizerId,
            entity.Title,
            entity.Description,
            entity.Category,
            entity.StartAtUtc,
            entity.EndAtUtc,
            entity.TimeZone,
            entity.LocationName,
            entity.LocationAddress,
            entity.OnlineMeetingUrl,
            entity.Capacity,
            entity.Status.ToString());
    }

    private async Task TryPromoteWaitlistedParticipant(Guid eventId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Events
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == eventId, cancellationToken);

        if (entity is null)
        {
            return;
        }

        var confirmedCount = await _dbContext.EventParticipants
            .CountAsync(x => x.EventId == eventId && x.Status == ParticipantStatus.Confirmed, cancellationToken);

        if (confirmedCount >= entity.Capacity)
        {
            return;
        }

        var waitlisted = await _dbContext.EventParticipants
            .Where(x => x.EventId == eventId && x.Status == ParticipantStatus.Waitlisted)
            .OrderBy(x => x.AddedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (waitlisted is null)
        {
            return;
        }

        waitlisted.Status = ParticipantStatus.Confirmed;

        var invite = await _dbContext.EventInvites
            .FirstOrDefaultAsync(x => x.EventId == eventId && x.Email == waitlisted.Email, cancellationToken);

        if (invite is not null)
        {
            invite.Status = InviteStatus.Accepted;
            invite.RespondedAtUtc = DateTime.UtcNow;
        }
    }
}

public sealed record CreateEventRequest(
    string Title,
    string? Description,
    string? Category,
    DateTime StartAtUtc,
    DateTime? EndAtUtc,
    string TimeZone,
    string? LocationName,
    string? LocationAddress,
    string? OnlineMeetingUrl,
    int Capacity);

public sealed record UpdateEventRequest(
    string Title,
    string? Description,
    string? Category,
    DateTime StartAtUtc,
    DateTime? EndAtUtc,
    string TimeZone,
    string? LocationName,
    string? LocationAddress,
    string? OnlineMeetingUrl,
    int Capacity);

public sealed record UpdateEventStatusRequest(string Status);

public sealed record InviteUsersRequest(IReadOnlyCollection<string> Emails);

public sealed record UpdateParticipantStatusRequest(string Status);

public sealed record EventInviteResponse(
    Guid Id,
    string Email,
    string Status,
    DateTime InvitedAtUtc,
    DateTime? RespondedAtUtc);

public sealed record RsvpRequest(string Decision);

public sealed record RsvpResponse(
    Guid EventId,
    string Email,
    string InviteStatus,
    string ParticipantStatus);

public sealed record EventParticipantResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string Status,
    DateTime AddedAtUtc);

public sealed record EventListItemResponse(
    Guid Id,
    string Title,
    DateTime StartAtUtc,
    DateTime? EndAtUtc,
    int Capacity,
    string Status);

public sealed record EventDetailResponse(
    Guid Id,
    Guid OrganizerId,
    string Title,
    string? Description,
    string? Category,
    DateTime StartAtUtc,
    DateTime? EndAtUtc,
    string TimeZone,
    string? LocationName,
    string? LocationAddress,
    string? OnlineMeetingUrl,
    int Capacity,
    string Status);

public sealed record EventSummaryResponse(
    int ActiveEventCount,
    int AcceptedGuestCount,
    int WaitlistCount,
    double FillRate,
    IReadOnlyCollection<EventSummaryItemResponse> UpcomingEvents);

public sealed record EventSummaryItemResponse(
    Guid Id,
    string Title,
    DateTime StartAtUtc,
    DateTime? EndAtUtc,
    string? LocationName,
    string? OnlineMeetingUrl,
    int Capacity,
    string Status,
    int AcceptedCount,
    int WaitlistCount);
