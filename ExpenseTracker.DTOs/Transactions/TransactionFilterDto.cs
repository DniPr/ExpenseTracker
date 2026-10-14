using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.DTOs.Transactions;

public class TransactionFilterDto
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public TransactionType? Type { get; set; }

    public int? CategoryId { get; set; }

    public string? SearchTerm { get; set; }
}