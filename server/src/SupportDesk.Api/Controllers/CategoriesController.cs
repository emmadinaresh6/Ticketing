using Microsoft.AspNetCore.Mvc;
using SupportDesk.Application.Categories;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(CategoryService categories) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CategoryDto>> List(CancellationToken ct) => categories.ListAsync(ct);

    // 201 with the new category, or 200 with the existing one when the name is a duplicate.
    [HttpPost]
    public async Task<ActionResult<AddCategoryResult>> Add(AddCategoryRequest request, CancellationToken ct)
    {
        var result = await categories.AddAsync(request, ct);
        return result.Created ? StatusCode(StatusCodes.Status201Created, result) : Ok(result);
    }
}
