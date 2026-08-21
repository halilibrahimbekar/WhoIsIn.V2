using Microsoft.AspNetCore.Mvc;
using WhoIsInV2.Application.Events;

namespace WhoIsInV2.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CategoryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var categories = await categoryService.GetAllAsync(cancellationToken);
        return Ok(categories.Select(item => new CategoryResponse(item.Id, item.Name)).ToArray());
    }
}

public sealed record CategoryResponse(Guid Id, string Name);