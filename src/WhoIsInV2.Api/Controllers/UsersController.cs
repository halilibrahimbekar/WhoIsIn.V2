using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WhoIsInV2.Domain.Entities;
using WhoIsInV2.Infrastructure.Persistence;

namespace WhoIsInV2.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly WhoIsInV2DbContext _dbContext;

    public UsersController(WhoIsInV2DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserListItemResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .OrderBy(x => x.Email)
            .Select(x => new UserListItemResponse(x.Id, x.Email, x.FirstName, x.LastName, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDetailResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UserDetailResponse(x.Id, x.Email, x.FirstName, x.LastName, x.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDetailResponse>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await _dbContext.Users
            .AnyAsync(x => x.Email == normalizedEmail, cancellationToken);

        if (exists)
        {
            return Conflict("A user with this email already exists.");
        }

        var entity = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Users.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new UserDetailResponse(entity.Id, entity.Email, entity.FirstName, entity.LastName, entity.CreatedAtUtc);

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
    }
}

public sealed record CreateUserRequest(string Email, string FirstName, string LastName);

public sealed record UserListItemResponse(Guid Id, string Email, string FirstName, string LastName, DateTime CreatedAtUtc);

public sealed record UserDetailResponse(Guid Id, string Email, string FirstName, string LastName, DateTime CreatedAtUtc);
