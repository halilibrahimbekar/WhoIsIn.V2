using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WhoIsInV2.Application.Events;

namespace WhoIsInV2.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(IEventService eventService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<EventListItemResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var events = await eventService.GetAllAsync(CurrentUserId(), cancellationToken);
        return Ok(events.Select(item => new EventListItemResponse(item.Id, item.Title, item.CategoryId, item.CategoryName, item.Visibility, item.StartAtUtc, item.EndAtUtc, item.Capacity, item.Status)).ToArray());
    }

    [Authorize]
    [HttpGet("summary")]
    public async Task<ActionResult<EventSummaryResponse>> GetSummary(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId(); if (userId is null) return Unauthorized();
        var summary = await eventService.GetSummaryAsync(userId.Value, cancellationToken);
        return Ok(new EventSummaryResponse(summary.ActiveEventCount, summary.AcceptedGuestCount, summary.WaitlistCount, summary.FillRate, summary.UpcomingEvents.Select(item => new EventSummaryItemResponse(item.Id, item.Title, item.StartAtUtc, item.EndAtUtc, item.LocationName, item.OnlineMeetingUrl, item.Capacity, item.Status, item.AcceptedCount, item.WaitlistCount)).ToArray()));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await eventService.GetByIdAsync(id, CurrentUserId(), cancellationToken);
        return item is null ? NotFound() : Ok(ToResponse(item));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<EventDetailResponse>> Create(CreateEventRequest request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId(); if (userId is null) return Unauthorized();
        var result = await eventService.CreateAsync(userId.Value, ToCommand(request), cancellationToken);
        return result.Status == EventOperationStatus.Success ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, ToResponse(result.Value)) : Map(result);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EventDetailResponse>> Update(Guid id, UpdateEventRequest request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId(); if (userId is null) return Unauthorized();
        var result = await eventService.UpdateAsync(userId.Value, id, ToCommand(request), cancellationToken);
        return result.Status == EventOperationStatus.Success ? Ok(ToResponse(result.Value!)) : Map(result);
    }

    [Authorize]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateEventStatusRequest request, CancellationToken cancellationToken) => await Execute(id, (userId, token) => eventService.UpdateStatusAsync(userId, id, request.Status, token), cancellationToken);

    [Authorize]
    [HttpGet("{id:guid}/invites")]
    public async Task<ActionResult<IReadOnlyCollection<EventInviteResponse>>> GetInvites(Guid id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await eventService.GetInvitesAsync(userId.Value, id, cancellationToken);
        return result.Status == EventOperationStatus.Success ? Ok(result.Value!.Select(item => new EventInviteResponse(item.Id, item.Email, item.Status, item.InvitedAtUtc, item.RespondedAtUtc)).ToArray()) : Map(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/invites")]
    public async Task<ActionResult<IReadOnlyCollection<EventInviteResponse>>> Invite(Guid id, InviteUsersRequest request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId(); if (userId is null) return Unauthorized(); var result = await eventService.InviteAsync(userId.Value, id, request.Emails, cancellationToken);
        return result.Status == EventOperationStatus.Success ? Ok(result.Value!.Select(item => new EventInviteResponse(item.Id, item.Email, item.Status, item.InvitedAtUtc, item.RespondedAtUtc)).ToArray()) : Map(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/rsvp")]
    public async Task<ActionResult<RsvpResponse>> Rsvp(Guid id, RsvpRequest request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId(); if (userId is null) return Unauthorized(); var result = await eventService.RsvpAsync(userId.Value, id, request.Decision, cancellationToken);
        return result.Status == EventOperationStatus.Success ? Ok(new RsvpResponse(result.Value!.EventId, result.Value.Email, result.Value.InviteStatus, result.Value.ParticipantStatus)) : Map(result);
    }

    [Authorize]
    [HttpGet("{id:guid}/participants")]
    public async Task<ActionResult<IReadOnlyCollection<EventParticipantResponse>>> GetParticipants(Guid id, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId(); if (userId is null) return Unauthorized(); var result = await eventService.GetParticipantsAsync(userId.Value, id, cancellationToken);
        return result.Status == EventOperationStatus.Success ? Ok(result.Value!.Select(item => new EventParticipantResponse(item.Id, item.Email, item.DisplayName, item.Status, item.AddedAtUtc)).ToArray()) : Map(result);
    }

    [Authorize]
    [HttpPatch("{id:guid}/participants/{participantId:guid}")]
    public async Task<IActionResult> UpdateParticipantStatus(Guid id, Guid participantId, UpdateParticipantStatusRequest request, CancellationToken cancellationToken) => await Execute(id, (userId, token) => eventService.UpdateParticipantStatusAsync(userId, id, participantId, request.Status, token), cancellationToken);

    [Authorize]
    [HttpPost("{id:guid}/waitlist/promote")]
    public async Task<IActionResult> PromoteWaitlistedParticipant(Guid id, CancellationToken cancellationToken) => await Execute(id, (userId, token) => eventService.PromoteWaitlistedAsync(userId, id, token), cancellationToken);

    private async Task<IActionResult> Execute(Guid id, Func<Guid, CancellationToken, Task<EventResult>> action, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId(); if (userId is null) return Unauthorized(); var result = await action(userId.Value, cancellationToken);
        return result.Status == EventOperationStatus.Success ? NoContent() : Map(result);
    }

    private Guid? CurrentUserId() { var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier); return Guid.TryParse(subject, out var id) ? id : null; }
    private static EventCommand ToCommand(CreateEventRequest item) => new(item.Title, item.Description, item.CategoryId, item.Visibility, item.RequireApproval, item.StartAtUtc, item.EndAtUtc, item.TimeZone, item.LocationName, item.LocationAddress, item.OnlineMeetingUrl, item.Capacity);
    private static EventCommand ToCommand(UpdateEventRequest item) => new(item.Title, item.Description, item.CategoryId, item.Visibility, item.RequireApproval, item.StartAtUtc, item.EndAtUtc, item.TimeZone, item.LocationName, item.LocationAddress, item.OnlineMeetingUrl, item.Capacity);
    private static EventDetailResponse ToResponse(EventDetail item) => new(item.Id, item.OrganizerId, item.Title, item.Description, item.CategoryId, item.CategoryName, item.Visibility, item.RequireApproval, item.StartAtUtc, item.EndAtUtc, item.TimeZone, item.LocationName, item.LocationAddress, item.OnlineMeetingUrl, item.Capacity, item.Status);
    private ActionResult Map(EventResult result) => result.Status switch { EventOperationStatus.BadRequest => BadRequest(result.Error), EventOperationStatus.Unauthorized => Unauthorized(), EventOperationStatus.Forbidden => Forbid(), EventOperationStatus.NotFound => result.Error is null ? NotFound() : NotFound(result.Error), EventOperationStatus.Conflict => Conflict(result.Error), _ => Problem(statusCode: 500) };
    private ActionResult Map<T>(EventResult<T> result) => Map((EventResult)result);
}

public sealed record CreateEventRequest(string Title, string? Description, Guid? CategoryId, string Visibility, bool RequireApproval, DateTime StartAtUtc, DateTime? EndAtUtc, string TimeZone, string? LocationName, string? LocationAddress, string? OnlineMeetingUrl, int Capacity);
public sealed record UpdateEventRequest(string Title, string? Description, Guid? CategoryId, string Visibility, bool RequireApproval, DateTime StartAtUtc, DateTime? EndAtUtc, string TimeZone, string? LocationName, string? LocationAddress, string? OnlineMeetingUrl, int Capacity);
public sealed record UpdateEventStatusRequest(string Status);
public sealed record InviteUsersRequest(IReadOnlyCollection<string> Emails);
public sealed record UpdateParticipantStatusRequest(string Status);
public sealed record EventInviteResponse(Guid Id, string Email, string Status, DateTime InvitedAtUtc, DateTime? RespondedAtUtc);
public sealed record RsvpRequest(string Decision);
public sealed record RsvpResponse(Guid EventId, string Email, string InviteStatus, string ParticipantStatus);
public sealed record EventParticipantResponse(Guid Id, string Email, string DisplayName, string Status, DateTime AddedAtUtc);
public sealed record EventListItemResponse(Guid Id, string Title, Guid? CategoryId, string? CategoryName, string Visibility, DateTime StartAtUtc, DateTime? EndAtUtc, int Capacity, string Status);
public sealed record EventDetailResponse(Guid Id, Guid OrganizerId, string Title, string? Description, Guid? CategoryId, string? CategoryName, string Visibility, bool RequireApproval, DateTime StartAtUtc, DateTime? EndAtUtc, string TimeZone, string? LocationName, string? LocationAddress, string? OnlineMeetingUrl, int Capacity, string Status);
public sealed record EventSummaryResponse(int ActiveEventCount, int AcceptedGuestCount, int WaitlistCount, double FillRate, IReadOnlyCollection<EventSummaryItemResponse> UpcomingEvents);
public sealed record EventSummaryItemResponse(Guid Id, string Title, DateTime StartAtUtc, DateTime? EndAtUtc, string? LocationName, string? OnlineMeetingUrl, int Capacity, string Status, int AcceptedCount, int WaitlistCount);