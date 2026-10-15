using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.DTOs.RecurringTransactions;

public class RecurringTransactionDto
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime NextExecutionDate { get; set; }

    public DateTime? EndDate { get; set; }

    public RecurringInterval Interval { get; set; }

    public bool IsActive { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public DateTime CreatedOn { get; set; }
}
