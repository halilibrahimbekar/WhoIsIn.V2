using Microsoft.AspNetCore.Mvc;
using WhoIsInV2.Application.Users;

namespace WhoIsInV2.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserListItemResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return Ok(users.Select(user => new UserListItemResponse(user.Id, user.Email, user.FirstName, user.LastName, user.CreatedAtUtc)).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(ToResponse(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserDetailResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await userService.CreateAsync(new CreateUserCommand(request.Email, request.FirstName, request.LastName), cancellationToken);
        if (result.EmailExists) return Conflict("A user with this email already exists.");
        var user = result.User!;
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToResponse(user));
    }

    private static UserDetailResponse ToResponse(UserDetail user) => new(user.Id, user.Email, user.FirstName, user.LastName, user.CreatedAtUtc);
}

public sealed record CreateUserRequest(string Email, string FirstName, string LastName);
public sealed record UserListItemResponse(Guid Id, string Email, string FirstName, string LastName, DateTime CreatedAtUtc);
public sealed record UserDetailResponse(Guid Id, string Email, string FirstName, string LastName, DateTime CreatedAtUtc);