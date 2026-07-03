namespace WhoIsInV2.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Event> OrganizedEvents { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
