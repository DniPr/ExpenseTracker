using System.Security.Claims;
using ExpenseTracker.DTOs.Budgets;
using ExpenseTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/budgets")]
[Authorize]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetService budgetService;

    public BudgetsController(IBudgetService budgetService)
    {
        this.budgetService = budgetService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        IEnumerable<BudgetDto> budgets =
            await budgetService.GetAllAsync(userId);

        return Ok(budgets);
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

        BudgetDto? budget =
            await budgetService.GetByIdAsync(id,userId);

        if (budget == null)
        {
            return NotFound();
        }

        return Ok(budget);
    }

    [HttpPost]
    public async Task<IActionResult> Create(BudgetCreateDto dto)
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        BudgetDto? budget =
            await budgetService.CreateAsync(dto,userId);

        if (budget == null)
        {
            return BadRequest(new
            {
                message = "Budget could not be created. Check the period, category, or duplicate budget."
            });
        }

        return CreatedAtAction(nameof(GetById),new { id = budget.Id },budget);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id,BudgetUpdateDto dto)
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        bool success =
            await budgetService.UpdateAsync(id,dto,userId);

        if (!success)
        {
            return BadRequest(new
            {
                message = "Budget could not be updated."
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
            await budgetService.DeleteAsync(
                id,
                userId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}