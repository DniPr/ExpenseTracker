using System.Security.Claims;
using ExpenseTracker.DTOs.RecurringTransactions;
using ExpenseTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/recurring-transactions")]
[Authorize]
public class RecurringTransactionsController : ControllerBase
{
    private readonly IRecurringTransactionService recurringTransactionService;

    public RecurringTransactionsController(IRecurringTransactionService recurringTransactionService)
    {
        this.recurringTransactionService = recurringTransactionService;
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

        IEnumerable<RecurringTransactionDto> recurringTransactions =
            await recurringTransactionService.GetAllAsync(userId);

        return Ok(recurringTransactions);
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

        RecurringTransactionDto? recurringTransaction =
            await recurringTransactionService.GetByIdAsync(id,userId);

        if (recurringTransaction == null)
        {
            return NotFound();
        }

        return Ok(recurringTransaction);
    }

    [HttpPost]
    public async Task<IActionResult> Create(RecurringTransactionCreateDto dto)
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        RecurringTransactionDto? recurringTransaction =
            await recurringTransactionService.CreateAsync(dto,userId);

        if (recurringTransaction == null)
        {
            return BadRequest(new
            {
                message = "Recurring transaction could not be created. Check the dates, category, and transaction type."
            });
        }

        return CreatedAtAction(nameof(GetById),new { id = recurringTransaction.Id },recurringTransaction);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id,RecurringTransactionUpdateDto dto)
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        bool success =
            await recurringTransactionService.UpdateAsync(id,dto,userId);

        if (!success)
        {
            return BadRequest(new
            {
                message = "Recurring transaction could not be updated."
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
            await recurringTransactionService.DeleteAsync(id,userId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}