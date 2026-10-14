using System.Security.Claims;
using ExpenseTracker.DTOs.Categories;
using ExpenseTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        this.categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        IEnumerable<CategoryDto> categories =
            await categoryService.GetAllAsync(userId);

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        CategoryDto? category =
            await categoryService.GetByIdAsync(id, userId);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryCreateDto dto)
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        CategoryDto? category =
            await categoryService.CreateAsync(dto,userId);

        if (category == null)
        {
            return BadRequest(new
            {
                message = "A category with the same name and type already exists."
            });
        }

        return CreatedAtAction(nameof(GetById),new { id = category.Id },category);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id,CategoryUpdateDto dto)
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        bool success =
            await categoryService.UpdateAsync(id,dto,userId);

        if (!success)
        {
            return BadRequest(new
            {
                message = "Category could not be updated."
            });
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        bool success =
            await categoryService.DeleteAsync(id,userId);

        if (!success)
        {
            return BadRequest(new
            {
                message = "Category could not be deleted."
            });
        }

        return NoContent();
    }
}