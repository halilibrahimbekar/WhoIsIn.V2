using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;

using WhoIsInV2.Application.Common.Interfaces;
using WhoIsInV2.Domain.Entities;

namespace WhoIsInV2.Application.Events;

public interface IEventService
{
    Task<PagedResult<EventListItem>> GetAllAsync(Guid? userId, EventListQuery query, CancellationToken cancellationToken);
    Task<EventSummary> GetSummaryAsync(Guid organizerId, CancellationToken cancellationToken);
    Task<EventDetail?> GetByIdAsync(Guid id, Guid? userId, CancellationToken cancellationToken);
    Task<EventResult<EventDetail>> CreateAsync(Guid organizerId, EventCommand command, CancellationToken cancellationToken);
    Task<EventResult<EventDetail>> UpdateAsync(Guid organizerId, Guid id, EventCommand command, CancellationToken cancellationToken);
    Task<EventResult> UpdateStatusAsync(Guid organizerId, Guid id, string status, CancellationToken cancellationToken);
    Task<EventResult<PagedResult<EventInviteItem>>> GetInvitesAsync(Guid organizerId, Guid id, PageQuery paging, CancellationToken cancellationToken);
    Task<EventResult<IReadOnlyCollection<EventInviteItem>>> InviteAsync(Guid organizerId, Guid id, IReadOnlyCollection<string> emails, CancellationToken cancellationToken);
    Task<EventResult<RsvpItem>> RsvpAsync(Guid userId, Guid id, string decision, CancellationToken cancellationToken);
    Task<EventResult<PagedResult<EventParticipantItem>>> GetParticipantsAsync(Guid organizerId, Guid id, PageQuery paging, CancellationToken cancellationToken);
    Task<EventResult> UpdateParticipantStatusAsync(Guid organizerId, Guid id, Guid participantId, string status, CancellationToken cancellationToken);
    Task<EventResult> PromoteWaitlistedAsync(Guid organizerId, Guid id, CancellationToken cancellationToken);
}

public sealed class EventService(IWhoIsInV2DbContext dbContext) : IEventService
{
    public async Task<PagedResult<EventListItem>> GetAllAsync(Guid? userId, EventListQuery query, CancellationToken cancellationToken)
    {
        var userEmail = await GetUserEmailAsync(userId, cancellationToken);
        var q = dbContext.Events.AsNoTracking()
            .Where(item => item.Status == EventStatus.Published && item.Visibility == EventVisibility.Public
                || (userId.HasValue && item.OrganizerId == userId.Value)
                || (userEmail != null && item.Invites.Any(invite => invite.Email == userEmail)));

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(item => item.Title.ToLower().Contains(search)
                || (item.Description != null && item.Description.ToLower().Contains(search))
                || (item.LocationName != null && item.LocationName.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<EventStatus>(query.Status, true, out var statusFilter))
        {
            q = q.Where(item => item.Status == statusFilter);
        }

        var total = await q.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var items = await q.OrderBy(item => item.StartAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new EventListItem(item.Id, item.Title, item.CategoryId, item.Category!.Name, item.Visibility.ToString(), item.StartAtUtc, item.EndAtUtc, item.Capacity, item.Status.ToString()))
            .ToListAsync(cancellationToken);

        return new PagedResult<EventListItem>(items, total, page, pageSize);
    }

    public async Task<EventSummary> GetSummaryAsync(Guid organizerId, CancellationToken cancellationToken)
    {
        var events = await dbContext.Events.AsNoTracking().Where(item => item.OrganizerId == organizerId).OrderBy(item => item.StartAtUtc)
            .Select(item => new EventSummaryItem(item.Id, item.Title, item.StartAtUtc, item.EndAtUtc, item.LocationName, item.OnlineMeetingUrl, item.Capacity, item.Status.ToString(),
                item.Participants.Count(participant => participant.Status == ParticipantStatus.Confirmed), item.Participants.Count(participant => participant.Status == ParticipantStatus.Waitlisted))).ToListAsync(cancellationToken);
        var accepted = events.Sum(item => item.AcceptedCount);
        var waitlisted = events.Sum(item => item.WaitlistCount);
        var capacity = events.Sum(item => item.Capacity);
        return new EventSummary(events.Count(item => item.Status is nameof(EventStatus.Draft) or nameof(EventStatus.Published)), accepted, waitlisted,
            capacity == 0 ? 0 : Math.Round(accepted * 100d / capacity, 1), events.Take(5).ToArray());
    }

    public async Task<EventDetail?> GetByIdAsync(Guid id, Guid? userId, CancellationToken cancellationToken)
    {
        var userEmail = await GetUserEmailAsync(userId, cancellationToken);
        return await dbContext.Events.AsNoTracking()
            .Where(item => item.Id == id
                && (item.Status == EventStatus.Published && item.Visibility == EventVisibility.Public
                    || (userId.HasValue && item.OrganizerId == userId.Value)
                    || (userEmail != null && item.Invites.Any(invite => invite.Email == userEmail))))
            .Select(ToDetail())
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<EventResult<EventDetail>> CreateAsync(Guid organizerId, EventCommand command, CancellationToken cancellationToken)
    {
        var validation = Validate(command, requireTitle: true);
        if (validation is not null) return EventResult<EventDetail>.BadRequest(validation);
        if (!Enum.TryParse<EventVisibility>(command.Visibility, true, out var visibility)) return EventResult<EventDetail>.BadRequest("Invalid event visibility.");
        if (command.CategoryId is not null && !await dbContext.Categories.AnyAsync(item => item.Id == command.CategoryId, cancellationToken)) return EventResult<EventDetail>.BadRequest("Category was not found.");
        if (!await dbContext.Users.AnyAsync(user => user.Id == organizerId, cancellationToken)) return EventResult<EventDetail>.Unauthorized();
        var now = DateTime.UtcNow;
        var entity = new Event
        {
            Id = Guid.NewGuid(),
            OrganizerId = organizerId,
            Title = command.Title.Trim(),
            Description = command.Description,
            CategoryId = command.CategoryId,
            Visibility = visibility,
            RequireApproval = command.RequireApproval,
            StartAtUtc = command.StartAtUtc,
            EndAtUtc = command.EndAtUtc,
            TimeZone = command.TimeZone,
            LocationName = command.LocationName,
            LocationAddress = command.LocationAddress,
            OnlineMeetingUrl = command.OnlineMeetingUrl,
            Capacity = command.Capacity,
            Status = EventStatus.Draft,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        dbContext.Events.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return EventResult<EventDetail>.Success(ToDetail(entity));
    }

    public async Task<EventResult<EventDetail>> UpdateAsync(Guid organizerId, Guid id, EventCommand command, CancellationToken cancellationToken)
    {
        var validation = Validate(command, requireTitle: true);
        if (validation is not null)
        {
            return EventResult<EventDetail>.BadRequest(validation);
        }

        if (!Enum.TryParse<EventVisibility>(command.Visibility, true, out var visibility)) return EventResult<EventDetail>.BadRequest("Invalid event visibility.");
        if (command.CategoryId is not null && !await dbContext.Categories.AnyAsync(item => item.Id == command.CategoryId, cancellationToken)) return EventResult<EventDetail>.BadRequest("Category was not found.");
        var entity = await dbContext.Events.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null) return EventResult<EventDetail>.NotFound();
        if (entity.OrganizerId != organizerId) return EventResult<EventDetail>.Forbidden();
        if (command.Capacity < await dbContext.EventParticipants.CountAsync(item => item.EventId == id && item.Status == ParticipantStatus.Confirmed, cancellationToken))
        {
            return EventResult<EventDetail>.Conflict("Capacity cannot be lower than the confirmed participant count.");
        }
        entity.Title = command.Title.Trim(); entity.Description = command.Description; entity.CategoryId = command.CategoryId; entity.Visibility = visibility; entity.RequireApproval = command.RequireApproval; entity.StartAtUtc = command.StartAtUtc;
        entity.EndAtUtc = command.EndAtUtc; entity.TimeZone = command.TimeZone; entity.LocationName = command.LocationName; entity.LocationAddress = command.LocationAddress;
        entity.OnlineMeetingUrl = command.OnlineMeetingUrl; entity.Capacity = command.Capacity; entity.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return EventResult<EventDetail>.Success(ToDetail(entity));
    }

    public async Task<EventResult> UpdateStatusAsync(Guid organizerId, Guid id, string status, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Events.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null) return EventResult.NotFound();
        if (entity.OrganizerId != organizerId) return EventResult.Forbidden();
        if (!Enum.TryParse<EventStatus>(status, true, out var nextStatus)) return EventResult.BadRequest("Invalid status value.");
        if (!IsValidStatusTransition(entity.Status, nextStatus)) return EventResult.BadRequest($"Cannot change event status from {entity.Status} to {nextStatus}.");
        entity.Status = nextStatus; entity.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return EventResult.Success();
    }

    public async Task<EventResult<PagedResult<EventInviteItem>>> GetInvitesAsync(Guid organizerId, Guid id, PageQuery paging, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Events.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null) return EventResult<PagedResult<EventInviteItem>>.NotFound();

        var userEmail = entity.OrganizerId == organizerId ? null : await GetUserEmailAsync(organizerId, cancellationToken);
        if (entity.OrganizerId != organizerId && userEmail is null)
        {
            return EventResult<PagedResult<EventInviteItem>>.Unauthorized();
        }

        var q = dbContext.EventInvites.AsNoTracking()
            .Where(item => item.EventId == id && (entity.OrganizerId == organizerId || item.Email == userEmail))
            .OrderBy(item => item.Email);

        var total = await q.CountAsync(cancellationToken);
        var page = Math.Max(1, paging.Page);
        var pageSize = Math.Clamp(paging.PageSize, 1, 100);
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new EventInviteItem(item.Id, item.Email, item.Status.ToString(), item.InvitedAtUtc, item.RespondedAtUtc)).ToListAsync(cancellationToken);

        return EventResult<PagedResult<EventInviteItem>>.Success(new PagedResult<EventInviteItem>(items, total, page, pageSize));
    }

    public async Task<EventResult<IReadOnlyCollection<EventInviteItem>>> InviteAsync(Guid organizerId, Guid id, IReadOnlyCollection<string> emails, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Events.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null) return EventResult<IReadOnlyCollection<EventInviteItem>>.NotFound();
        if (entity.OrganizerId != organizerId) return EventResult<IReadOnlyCollection<EventInviteItem>>.Forbidden();
        var normalized = emails.Select(email => email.Trim().ToLowerInvariant()).Where(email => !string.IsNullOrWhiteSpace(email)).Distinct(StringComparer.Ordinal).ToArray();
        if (normalized.Length == 0) return EventResult<IReadOnlyCollection<EventInviteItem>>.BadRequest("At least one valid email is required.");
        var existing = await dbContext.EventInvites.Where(item => item.EventId == id && normalized.Contains(item.Email)).Select(item => item.Email).ToListAsync(cancellationToken);
        var existingSet = existing.ToHashSet(StringComparer.Ordinal);
        var created = normalized.Where(email => !existingSet.Contains(email)).Select(email => new EventInvite
        {
            Id = Guid.NewGuid(),
            EventId = id,
            Email = email,
            InviteToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
            Status = InviteStatus.Pending,
            InvitedAtUtc = DateTime.UtcNow
        }).ToList();
        if (created.Count > 0) { dbContext.EventInvites.AddRange(created); await dbContext.SaveChangesAsync(cancellationToken); }
        return EventResult<IReadOnlyCollection<EventInviteItem>>.Success(created.OrderBy(item => item.Email).Select(item => new EventInviteItem(item.Id, item.Email, item.Status.ToString(), item.InvitedAtUtc, item.RespondedAtUtc)).ToList());
    }

    public async Task<EventResult<RsvpItem>> RsvpAsync(Guid userId, Guid id, string decision, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            return EventResult<RsvpItem>.Unauthorized();
        }

        var entity = await dbContext.Events.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null)
        {
            return EventResult<RsvpItem>.NotFound();
        }

        if (entity.Status != EventStatus.Published)
        {
            return EventResult<RsvpItem>.BadRequest("RSVP is only available for published events.");
        }

        var invite = await dbContext.EventInvites.SingleOrDefaultAsync(item => item.EventId == id && item.Email == user.Email, cancellationToken);
        if (entity.Visibility == EventVisibility.InviteOnly && invite is null)
        {
            return EventResult<RsvpItem>.BadRequest("No invite found for this user and event.");
        }

        if (!Enum.TryParse<InviteStatus>(decision, true, out var parsed) || parsed is not (InviteStatus.Accepted or InviteStatus.Declined))
        {
            return EventResult<RsvpItem>.BadRequest("Decision must be Accepted or Declined.");
        }

        var participant = await dbContext.EventParticipants.SingleOrDefaultAsync(item => item.EventId == id && item.Email == user.Email, cancellationToken);
        var participantStatus = parsed == InviteStatus.Declined ? ParticipantStatus.Declined : entity.RequireApproval ? ParticipantStatus.PendingApproval : await GetParticipantStatusAsync(id, entity.Capacity, cancellationToken);
        if (invite is not null)
        {
            invite.Status = participantStatus == ParticipantStatus.Confirmed ? InviteStatus.Accepted : participantStatus == ParticipantStatus.Waitlisted ? InviteStatus.Waitlisted : InviteStatus.Pending;
            invite.RespondedAtUtc = DateTime.UtcNow;
        }
        if (participant is null)
        {
            dbContext.EventParticipants.Add(new EventParticipant { Id = Guid.NewGuid(), EventId = id, UserId = user.Id, DisplayName = $"{user.FirstName} {user.LastName}".Trim(), Email = user.Email, Status = participantStatus, AddedAtUtc = DateTime.UtcNow });
        }
        else
        {
            participant.Status = participantStatus;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        if (participantStatus == ParticipantStatus.Declined)
        {
            await PromoteFirstAsync(id, cancellationToken);
        }
        if (participantStatus == ParticipantStatus.Declined)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return EventResult<RsvpItem>.Success(new RsvpItem(id, user.Email, invite?.Status.ToString() ?? "Accepted", participantStatus.ToString()));
    }

    public async Task<EventResult<PagedResult<EventParticipantItem>>> GetParticipantsAsync(Guid organizerId, Guid id, PageQuery paging, CancellationToken cancellationToken)
    {
        var ownership = await CheckOwnershipAsync(organizerId, id, cancellationToken);
        if (ownership is not null)
        {
            return EventResult<PagedResult<EventParticipantItem>>.From(ownership);
        }

        var q = dbContext.EventParticipants.AsNoTracking().Where(item => item.EventId == id).OrderBy(item => item.AddedAtUtc);
        var total = await q.CountAsync(cancellationToken);
        var page = Math.Max(1, paging.Page);
        var pageSize = Math.Clamp(paging.PageSize, 1, 100);
        var participants = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new EventParticipantItem(item.Id, item.Email, item.DisplayName, item.Status.ToString(), item.AddedAtUtc)).ToListAsync(cancellationToken);
        return EventResult<PagedResult<EventParticipantItem>>.Success(new PagedResult<EventParticipantItem>(participants, total, page, pageSize));
    }

    public async Task<EventResult> UpdateParticipantStatusAsync(Guid organizerId, Guid id, Guid participantId, string status, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Events.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null)
        {
            return EventResult.NotFound();
        }

        if (entity.OrganizerId != organizerId)
        {
            return EventResult.Forbidden();
        }

        if (!Enum.TryParse<ParticipantStatus>(status, true, out var nextStatus))
        {
            return EventResult.BadRequest("Invalid participant status.");
        }

        var participant = await dbContext.EventParticipants.SingleOrDefaultAsync(item => item.Id == participantId && item.EventId == id, cancellationToken);
        if (participant is null)
        {
            return EventResult.NotFound();
        }

        if (nextStatus == ParticipantStatus.Confirmed && participant.Status != ParticipantStatus.Confirmed && await dbContext.EventParticipants.CountAsync(item => item.EventId == id && item.Status == ParticipantStatus.Confirmed, cancellationToken) >= entity.Capacity)
        {
            return EventResult.Conflict("Event capacity has been reached.");
        }
        participant.Status = nextStatus;
        var invite = await dbContext.EventInvites.SingleOrDefaultAsync(item => item.EventId == id && item.Email == participant.Email, cancellationToken);
        if (invite is not null)
        {
            invite.Status = nextStatus == ParticipantStatus.Confirmed ? InviteStatus.Accepted : nextStatus == ParticipantStatus.Waitlisted ? InviteStatus.Waitlisted : InviteStatus.Declined;
            invite.RespondedAtUtc = DateTime.UtcNow;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return EventResult.Success();
    }

    public async Task<EventResult> PromoteWaitlistedAsync(Guid organizerId, Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Events.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (entity is null)
        {
            return EventResult.NotFound();
        }

        if (entity.OrganizerId != organizerId)
        {
            return EventResult.Forbidden();
        }

        if (await dbContext.EventParticipants.CountAsync(item => item.EventId == id && item.Status == ParticipantStatus.Confirmed, cancellationToken) >= entity.Capacity)
        {
            return EventResult.Conflict("Event capacity has been reached.");
        }

        var promoted = await PromoteFirstAsync(id, cancellationToken);
        if (!promoted)
        {
            return EventResult.NotFound("No waitlisted participant found.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return EventResult.Success();
    }

    private async Task<EventResult?> CheckOwnershipAsync(Guid organizerId, Guid id, CancellationToken cancellationToken)
    {
        if (await dbContext.Events.AnyAsync(item => item.Id == id && item.OrganizerId == organizerId, cancellationToken))
        {
            return null;
        }

        return await dbContext.Events.AnyAsync(item => item.Id == id, cancellationToken) ? EventResult.Forbidden() : EventResult.NotFound();
    }

    private async Task<string?> GetUserEmailAsync(Guid? userId, CancellationToken cancellationToken) => userId is null
        ? null
        : await dbContext.Users.Where(user => user.Id == userId.Value).Select(user => user.Email).SingleOrDefaultAsync(cancellationToken);

    private async Task<ParticipantStatus> GetParticipantStatusAsync(Guid eventId, int capacity, CancellationToken cancellationToken) =>
        await dbContext.EventParticipants.CountAsync(item => item.EventId == eventId && item.Status == ParticipantStatus.Confirmed, cancellationToken) < capacity ? ParticipantStatus.Confirmed : ParticipantStatus.Waitlisted;

    private async Task<bool> PromoteFirstAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var eventCapacity = await dbContext.Events.Where(item => item.Id == eventId).Select(item => (int?)item.Capacity).SingleOrDefaultAsync(cancellationToken);
        if (eventCapacity is null || await dbContext.EventParticipants.CountAsync(item => item.EventId == eventId && item.Status == ParticipantStatus.Confirmed, cancellationToken) >= eventCapacity.Value)
        {
            return false;
        }

        var participant = await dbContext.EventParticipants.Where(item => item.EventId == eventId && item.Status == ParticipantStatus.Waitlisted).OrderBy(item => item.AddedAtUtc).FirstOrDefaultAsync(cancellationToken);
        if (participant is null)
        {
            return false;
        }

        participant.Status = ParticipantStatus.Confirmed;
        var invite = await dbContext.EventInvites.FirstOrDefaultAsync(item => item.EventId == eventId && item.Email == participant.Email, cancellationToken);
        if (invite is not null)
        {
            invite.Status = InviteStatus.Accepted;
            invite.RespondedAtUtc = DateTime.UtcNow;
        }
        return true;
    }

    private static string? Validate(EventCommand command, bool requireTitle) => requireTitle && string.IsNullOrWhiteSpace(command.Title) ? "Title is required." : command.Capacity <= 0 ? "Capacity must be greater than zero." : command.EndAtUtc is not null && command.EndAtUtc <= command.StartAtUtc ? "EndAtUtc must be greater than StartAtUtc." : null;
    private static bool IsValidStatusTransition(EventStatus current, EventStatus next) => current == next || current switch { EventStatus.Draft => next is EventStatus.Published or EventStatus.Cancelled, EventStatus.Published => next is EventStatus.Cancelled or EventStatus.Completed, _ => false };
    private static EventDetail ToDetail(Event entity) => new(entity.Id, entity.OrganizerId, entity.Title, entity.Description, entity.CategoryId, entity.Category?.Name, entity.Visibility.ToString(), entity.RequireApproval, entity.StartAtUtc, entity.EndAtUtc, entity.TimeZone, entity.LocationName, entity.LocationAddress, entity.OnlineMeetingUrl, entity.Capacity, entity.Status.ToString());
    private static System.Linq.Expressions.Expression<Func<Event, EventDetail>> ToDetail() => entity => new EventDetail(entity.Id, entity.OrganizerId, entity.Title, entity.Description, entity.CategoryId, entity.Category!.Name, entity.Visibility.ToString(), entity.RequireApproval, entity.StartAtUtc, entity.EndAtUtc, entity.TimeZone, entity.LocationName, entity.LocationAddress, entity.OnlineMeetingUrl, entity.Capacity, entity.Status.ToString());
}

public sealed record EventCommand(string Title, string? Description, Guid? CategoryId, string Visibility, bool RequireApproval, DateTime StartAtUtc, DateTime? EndAtUtc, string TimeZone, string? LocationName, string? LocationAddress, string? OnlineMeetingUrl, int Capacity);
public sealed record EventListQuery(int Page = 1, int PageSize = 20, string? Search = null, string? Status = null);
public sealed record PageQuery(int Page = 1, int PageSize = 20);
public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
public sealed record EventListItem(Guid Id, string Title, Guid? CategoryId, string? CategoryName, string Visibility, DateTime StartAtUtc, DateTime? EndAtUtc, int Capacity, string Status);
public sealed record EventDetail(Guid Id, Guid OrganizerId, string Title, string? Description, Guid? CategoryId, string? CategoryName, string Visibility, bool RequireApproval, DateTime StartAtUtc, DateTime? EndAtUtc, string TimeZone, string? LocationName, string? LocationAddress, string? OnlineMeetingUrl, int Capacity, string Status);
public sealed record EventSummary(int ActiveEventCount, int AcceptedGuestCount, int WaitlistCount, double FillRate, IReadOnlyCollection<EventSummaryItem> UpcomingEvents);
public sealed record EventSummaryItem(Guid Id, string Title, DateTime StartAtUtc, DateTime? EndAtUtc, string? LocationName, string? OnlineMeetingUrl, int Capacity, string Status, int AcceptedCount, int WaitlistCount);
public sealed record EventInviteItem(Guid Id, string Email, string Status, DateTime InvitedAtUtc, DateTime? RespondedAtUtc);
public sealed record EventParticipantItem(Guid Id, string Email, string DisplayName, string Status, DateTime AddedAtUtc);
public sealed record RsvpItem(Guid EventId, string Email, string InviteStatus, string ParticipantStatus);
public enum EventOperationStatus { Success, BadRequest, Unauthorized, Forbidden, NotFound, Conflict }
public class EventResult(EventOperationStatus status, string? error = null)
{
    public EventOperationStatus Status { get; } = status; public string? Error { get; } = error;
    public static EventResult Success() => new(EventOperationStatus.Success); public static EventResult BadRequest(string error) => new(EventOperationStatus.BadRequest, error); public static EventResult Unauthorized() => new(EventOperationStatus.Unauthorized); public static EventResult Forbidden() => new(EventOperationStatus.Forbidden); public static EventResult NotFound(string? error = null) => new(EventOperationStatus.NotFound, error); public static EventResult Conflict(string error) => new(EventOperationStatus.Conflict, error);
}
public sealed class EventResult<T>(EventOperationStatus status, T? value = default, string? error = null) : EventResult(status, error)
{
    public T? Value { get; } = value; public static EventResult<T> Success(T value) => new(EventOperationStatus.Success, value); public static new EventResult<T> BadRequest(string error) => new(EventOperationStatus.BadRequest, error: error); public static new EventResult<T> Unauthorized() => new(EventOperationStatus.Unauthorized); public static new EventResult<T> Forbidden() => new(EventOperationStatus.Forbidden); public static EventResult<T> NotFound() => new(EventOperationStatus.NotFound); public static new EventResult<T> Conflict(string error) => new(EventOperationStatus.Conflict, error: error); public static EventResult<T> From(EventResult result) => new(result.Status, error: result.Error);
}