using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.DTOs.Transactions;

public class TransactionCreateDto
{
    [Required]
    [MaxLength(150)]
    public string Description { get; set; } = null!;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Required]
    public int CategoryId { get; set; }
}