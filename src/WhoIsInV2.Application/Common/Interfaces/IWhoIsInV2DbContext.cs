using Microsoft.EntityFrameworkCore;
using WhoIsInV2.Domain.Entities;

namespace WhoIsInV2.Application.Common.Interfaces;

public interface IWhoIsInV2DbContext
{
    DbSet<User> Users { get; }
    DbSet<Event> Events { get; }
    DbSet<EventInvite> EventInvites { get; }
    DbSet<EventParticipant> EventParticipants { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}