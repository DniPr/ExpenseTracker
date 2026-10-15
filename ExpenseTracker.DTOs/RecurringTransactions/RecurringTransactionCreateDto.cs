using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.DTOs.RecurringTransactions;

public class RecurringTransactionCreateDto
{
    [Required]
    [MaxLength(150)]
    public string Description { get; set; } = null!;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public RecurringInterval Interval { get; set; }

    [Required]
    public int CategoryId { get; set; }
}