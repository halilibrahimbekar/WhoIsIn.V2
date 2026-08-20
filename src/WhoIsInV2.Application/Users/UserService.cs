using Microsoft.EntityFrameworkCore;
using WhoIsInV2.Application.Common.Interfaces;
using WhoIsInV2.Domain.Entities;

namespace WhoIsInV2.Application.Users;

public interface IUserService
{
    Task<IReadOnlyCollection<UserListItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserDetail?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CreateUserResult> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken);
}

public sealed class UserService(IWhoIsInV2DbContext dbContext) : IUserService
{
    public async Task<IReadOnlyCollection<UserListItem>> GetAllAsync(CancellationToken cancellationToken) => await dbContext.Users
        .AsNoTracking().OrderBy(user => user.Email)
        .Select(user => new UserListItem(user.Id, user.Email, user.FirstName, user.LastName, user.CreatedAtUtc))
        .ToListAsync(cancellationToken);

    public Task<UserDetail?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => dbContext.Users
        .AsNoTracking().Where(user => user.Id == id)
        .Select(user => new UserDetail(user.Id, user.Email, user.FirstName, user.LastName, user.CreatedAtUtc))
        .SingleOrDefaultAsync(cancellationToken);

    public async Task<CreateUserResult> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();
        if (await dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken)) return CreateUserResult.EmailAlreadyExists();
        var user = new User { Id = Guid.NewGuid(), Email = email, FirstName = command.FirstName.Trim(), LastName = command.LastName.Trim(), CreatedAtUtc = DateTime.UtcNow };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreateUserResult.Success(new UserDetail(user.Id, user.Email, user.FirstName, user.LastName, user.CreatedAtUtc));
    }
}

public sealed record CreateUserCommand(string Email, string FirstName, string LastName);
public sealed record UserListItem(Guid Id, string Email, string FirstName, string LastName, DateTime CreatedAtUtc);
public sealed record UserDetail(Guid Id, string Email, string FirstName, string LastName, DateTime CreatedAtUtc);
public sealed record CreateUserResult(UserDetail? User, bool EmailExists)
{
    public static CreateUserResult Success(UserDetail user) => new(user, false);
    public static CreateUserResult EmailAlreadyExists() => new(null, true);
}