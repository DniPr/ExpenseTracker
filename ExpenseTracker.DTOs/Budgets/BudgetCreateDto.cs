using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.DTOs.Budgets;

public class BudgetCreateDto
{
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public BudgetPeriod Period { get; set; }

    [Range(1, 12)]
    public int? Month { get; set; }

    [Range(2000, 2100)]
    public int Year { get; set; }

    public int? CategoryId { get; set; }
}