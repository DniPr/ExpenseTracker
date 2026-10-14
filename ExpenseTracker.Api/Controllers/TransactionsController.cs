using System.Security.Claims;
using ExpenseTracker.DTOs.Transactions;
using ExpenseTracker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        this.transactionService = transactionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TransactionFilterDto filter)
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        IEnumerable<TransactionDto> transactions =
            await transactionService.GetAllAsync(filter,userId);

        return Ok(transactions);
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

        TransactionDto? transaction =
            await transactionService.GetByIdAsync(id,userId);

        if (transaction == null)
        {
            return NotFound();
        }

        return Ok(transaction);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TransactionCreateDto dto)
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        TransactionDto? transaction =
            await transactionService.CreateAsync(dto,userId);

        if (transaction == null)
        {
            return BadRequest(new
            {
                message = "Transaction could not be created. Check the category and transaction type."
            });
        }

        return CreatedAtAction(nameof(GetById),new { id = transaction.Id },transaction);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id,TransactionUpdateDto dto)
    {
        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        bool success =
            await transactionService.UpdateAsync(id,dto,userId);

        if (!success)
        {
            return BadRequest(new
            {
                message = "Transaction could not be updated."
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
            await transactionService.DeleteAsync(
                id,
                userId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}