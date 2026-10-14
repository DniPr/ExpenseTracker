using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.DTOs.Transactions;

public class TransactionDto
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public TransactionType Type { get; set; }

    public string? Notes { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }
}