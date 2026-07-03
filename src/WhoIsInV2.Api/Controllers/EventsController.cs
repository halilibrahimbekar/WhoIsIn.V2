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
        if (request.Capacity <= 0)
        {
            return BadRequest("Capacity must be greater than zero.");
        }

        if (request.EndAtUtc is not null && request.EndAtUtc <= request.StartAtUtc)
        {
            return BadRequest("EndAtUtc must be greater than StartAtUtc.");
        }

        var organizerExists = await _dbContext.Users
            .AnyAsync(x => x.Id == request.OrganizerId, cancellationToken);

        if (!organizerExists)
        {
            return BadRequest("Organizer does not exist.");
        }

        var entity = new Event
        {
            Id = Guid.NewGuid(),
            OrganizerId = request.OrganizerId,
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
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateEventStatusRequest request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Events.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        if (!Enum.TryParse<EventStatus>(request.Status, ignoreCase: true, out var nextStatus))
        {
            return BadRequest("Invalid status value.");
        }

        entity.Status = nextStatus;
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
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

    [HttpGet("{id:guid}/participants")]
    public async Task<ActionResult<IReadOnlyCollection<EventParticipantResponse>>> GetParticipants(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Events.AnyAsync(x => x.Id == id, cancellationToken);
        if (!exists)
        {
            return NotFound();
        }

        var participants = await _dbContext.EventParticipants
            .AsNoTracking()
            .Where(x => x.EventId == id)
            .OrderBy(x => x.AddedAtUtc)
            .Select(x => new EventParticipantResponse(x.Id, x.Email, x.DisplayName, x.Status.ToString(), x.AddedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(participants);
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var userId) ? userId : null;
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
    int Capacity);

public sealed record UpdateEventStatusRequest(string Status);

public sealed record InviteUsersRequest(IReadOnlyCollection<string> Emails);

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
