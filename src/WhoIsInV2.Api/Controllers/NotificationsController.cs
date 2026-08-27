using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhoIsInV2.Domain.Entities;
using WhoIsInV2.Infrastructure.Persistence;

namespace WhoIsInV2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController(WhoIsInV2DbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<NotificationResponse>>> Get(CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out var userId)) return Unauthorized();

        var notifications = await dbContext.EventParticipants.AsNoTracking()
            .Where(item => item.Status == ParticipantStatus.PendingApproval && item.Event!.OrganizerId == userId)
            .OrderByDescending(item => item.AddedAtUtc)
            .Select(item => new NotificationResponse(item.Id, item.EventId, item.Event!.Title,
                $"{item.DisplayName} etkinliğinize katılım isteği gönderdi.", item.AddedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(notifications);
    }
}

public sealed record NotificationResponse(Guid Id, Guid EventId, string EventTitle, string Message, DateTime CreatedAtUtc);