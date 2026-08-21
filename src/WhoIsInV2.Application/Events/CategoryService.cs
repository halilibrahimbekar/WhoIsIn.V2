using Microsoft.EntityFrameworkCore;
using WhoIsInV2.Application.Common.Interfaces;

namespace WhoIsInV2.Application.Events;

public interface ICategoryService
{
    Task<IReadOnlyCollection<CategoryItem>> GetAllAsync(CancellationToken cancellationToken);
}

public sealed class CategoryService(IWhoIsInV2DbContext dbContext) : ICategoryService
{
    public async Task<IReadOnlyCollection<CategoryItem>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Categories.AsNoTracking().OrderBy(item => item.Name)
            .Select(item => new CategoryItem(item.Id, item.Name)).ToListAsync(cancellationToken);
}

public sealed record CategoryItem(Guid Id, string Name);