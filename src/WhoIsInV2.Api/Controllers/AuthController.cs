using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhoIsInV2.Application.Authentication;

namespace WhoIsInV2.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<ActionResult<AuthTokenResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(new RegisterCommand(request.Email, request.Password, request.FirstName, request.LastName), cancellationToken);
        return result.Status == AuthOperationStatus.EmailAlreadyExists ? Conflict("A user with this email already exists.") : ToActionResult(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthTokenResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(new LoginCommand(request.Email, request.Password), cancellationToken);
        return result.Status == AuthOperationStatus.InvalidCredentials ? Unauthorized("Invalid email or password.") : ToActionResult(result);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokenResponse>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken, cancellationToken);
        return result.Status == AuthOperationStatus.InvalidRefreshToken ? Unauthorized("Refresh token is no longer valid.") : ToActionResult(result);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(RefreshRequest request, CancellationToken cancellationToken) =>
        await _authService.RevokeAsync(request.RefreshToken, cancellationToken) == AuthOperationStatus.NotFound ? NotFound() : NoContent();

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me(CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out var userId)) return Unauthorized();
        var user = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        return user is null ? Unauthorized() : Ok(new CurrentUserResponse(user.Id, user.Email, user.FirstName, user.LastName, user.CreatedAtUtc));
    }

    private ActionResult<AuthTokenResponse> ToActionResult(AuthOperationResult result)
    {
        if (result.Status != AuthOperationStatus.Success || result.Session is null) return Problem("Authentication operation failed.", statusCode: 500);
        var session = result.Session;
        return Ok(new AuthTokenResponse(session.AccessToken, session.AccessTokenExpiresAtUtc, session.RefreshToken, session.RefreshTokenExpiresAtUtc,
            new CurrentUserResponse(session.User.Id, session.User.Email, session.User.FirstName, session.User.LastName, session.User.CreatedAtUtc)));
    }
}

public sealed record RegisterRequest(string Email, string Password, string FirstName, string LastName);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record AuthTokenResponse(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc, CurrentUserResponse User);
public sealed record CurrentUserResponse(Guid Id, string Email, string FirstName, string LastName, DateTime CreatedAtUtc);