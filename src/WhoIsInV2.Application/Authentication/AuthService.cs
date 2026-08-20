using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using WhoIsInV2.Application.Common.Interfaces;
using WhoIsInV2.Domain.Entities;

namespace WhoIsInV2.Application.Authentication;

public interface IAuthService
{
    Task<AuthOperationResult> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken);
    Task<AuthOperationResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
    Task<AuthOperationResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task<AuthOperationStatus> RevokeAsync(string refreshToken, CancellationToken cancellationToken);
    Task<CurrentUser?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}

public sealed class AuthService : IAuthService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    private readonly IWhoIsInV2DbContext _dbContext;
    private readonly IAccessTokenService _tokenService;

    public AuthService(IWhoIsInV2DbContext dbContext, IAccessTokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<AuthOperationResult> RegisterAsync(RegisterCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        if (await _dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            return AuthOperationResult.EmailAlreadyExists();
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = HashPassword(command.Password),
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<AuthOperationResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.SingleOrDefaultAsync(item => item.Email == email, cancellationToken);

        return user is null || !VerifyPassword(command.Password, user.PasswordHash)
            ? AuthOperationResult.InvalidCredentials()
            : await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<AuthOperationResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.ComputeTokenHash(refreshToken);
        var storedToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken?.User is null || storedToken.RevokedAtUtc is not null || storedToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return AuthOperationResult.InvalidRefreshToken();
        }

        var tokenPair = _tokenService.GenerateTokenPair(storedToken.User);
        storedToken.RevokedAtUtc = DateTime.UtcNow;
        storedToken.ReplacedByTokenHash = tokenPair.RefreshTokenHash;
        AddRefreshToken(storedToken.User.Id, tokenPair);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return AuthOperationResult.Success(ToSession(storedToken.User, tokenPair));
    }

    public async Task<AuthOperationStatus> RevokeAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.ComputeTokenHash(refreshToken);
        var storedToken = await _dbContext.RefreshTokens.SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
        if (storedToken is null)
        {
            return AuthOperationStatus.NotFound;
        }

        if (storedToken.RevokedAtUtc is null)
        {
            storedToken.RevokedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return AuthOperationStatus.Success;
    }

    public Task<CurrentUser?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken) =>
        _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new CurrentUser(user.Id, user.Email, user.FirstName, user.LastName, user.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

    private async Task<AuthOperationResult> CreateSessionAsync(User user, CancellationToken cancellationToken)
    {
        var tokenPair = _tokenService.GenerateTokenPair(user);
        AddRefreshToken(user.Id, tokenPair);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return AuthOperationResult.Success(ToSession(user, tokenPair));
    }

    private void AddRefreshToken(Guid userId, AccessTokenPair tokenPair)
    {
        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenPair.RefreshTokenHash,
            ExpiresAtUtc = tokenPair.RefreshTokenExpiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        });
    }

    private static AuthSession ToSession(User user, AccessTokenPair tokenPair) => new(
        tokenPair.AccessToken,
        tokenPair.AccessTokenExpiresAtUtc,
        tokenPair.RefreshToken,
        tokenPair.RefreshTokenExpiresAtUtc,
        new CurrentUser(user.Id, user.Email, user.FirstName, user.LastName, user.CreatedAtUtc));

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    private static bool VerifyPassword(string password, string? passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var parts = passwordHash.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[1]);
            var expectedKey = Convert.FromBase64String(parts[2]);
            var actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedKey.Length);
            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

public sealed record RegisterCommand(string Email, string Password, string FirstName, string LastName);
public sealed record LoginCommand(string Email, string Password);
public sealed record CurrentUser(Guid Id, string Email, string FirstName, string LastName, DateTime CreatedAtUtc);
public sealed record AuthSession(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc, CurrentUser User);
public sealed record AuthOperationResult(AuthOperationStatus Status, AuthSession? Session)
{
    public static AuthOperationResult Success(AuthSession session) => new(AuthOperationStatus.Success, session);
    public static AuthOperationResult EmailAlreadyExists() => new(AuthOperationStatus.EmailAlreadyExists, null);
    public static AuthOperationResult InvalidCredentials() => new(AuthOperationStatus.InvalidCredentials, null);
    public static AuthOperationResult InvalidRefreshToken() => new(AuthOperationStatus.InvalidRefreshToken, null);
}

public enum AuthOperationStatus
{
    Success,
    EmailAlreadyExists,
    InvalidCredentials,
    InvalidRefreshToken,
    NotFound
}