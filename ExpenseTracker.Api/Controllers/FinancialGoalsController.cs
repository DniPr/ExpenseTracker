using System.Security.Claims;
using ExpenseTracker.DTOs.FinancialGoals;
using ExpenseTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/financial-goals")]
[Authorize]
public class FinancialGoalsController : ControllerBase
{
    private readonly IFinancialGoalService financialGoalService;

    public FinancialGoalsController(IFinancialGoalService financialGoalService)
    {
        this.financialGoalService = financialGoalService;
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

        IEnumerable<FinancialGoalDto> goals =
            await financialGoalService.GetAllAsync(userId);

        return Ok(goals);
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

        FinancialGoalDto? goal =
            await financialGoalService.GetByIdAsync(id,userId);

        if (goal == null)
        {
            return NotFound();
        }

        return Ok(goal);
    }

    [HttpPost]
    public async Task<IActionResult> Create(FinancialGoalCreateDto dto)
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        FinancialGoalDto? goal =
            await financialGoalService.CreateAsync(dto,userId);

        if (goal == null)
        {
            return BadRequest(new
            {
                message = "Financial goal could not be created. Check the amounts and target date."
            });
        }

        return CreatedAtAction(nameof(GetById),new { id = goal.Id },goal);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id,FinancialGoalUpdateDto dto)
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        bool success =
            await financialGoalService.UpdateAsync(id,dto,userId);

        if (!success)
        {
            return BadRequest(new
            {
                message = "Financial goal could not be updated."
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
            await financialGoalService.DeleteAsync(id,userId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}