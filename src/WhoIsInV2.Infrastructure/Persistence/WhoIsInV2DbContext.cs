using Microsoft.EntityFrameworkCore;
using WhoIsInV2.Application.Common.Interfaces;
using WhoIsInV2.Domain.Entities;

namespace WhoIsInV2.Infrastructure.Persistence;

public class WhoIsInV2DbContext : DbContext, IWhoIsInV2DbContext
{
    public WhoIsInV2DbContext(DbContextOptions<WhoIsInV2DbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventInvite> EventInvites => Set<EventInvite>();
    public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.PasswordHash).HasMaxLength(512);
            entity.Property(x => x.FirstName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(120).IsRequired();
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(100);
            entity.Property(x => x.TimeZone).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LocationName).HasMaxLength(200);
            entity.Property(x => x.LocationAddress).HasMaxLength(500);
            entity.Property(x => x.OnlineMeetingUrl).HasMaxLength(500);

            entity.HasOne(x => x.Organizer)
                .WithMany(x => x.OrganizedEvents)
                .HasForeignKey(x => x.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EventInvite>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.InviteToken).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.InviteToken).IsUnique();

            entity.HasOne(x => x.Event)
                .WithMany(x => x.Invites)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EventParticipant>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => new { x.EventId, x.Email }).IsUnique();

            entity.HasOne(x => x.Event)
                .WithMany(x => x.Participants)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.Property(x => x.ReplacedByTokenHash).HasMaxLength(128);

            entity.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
