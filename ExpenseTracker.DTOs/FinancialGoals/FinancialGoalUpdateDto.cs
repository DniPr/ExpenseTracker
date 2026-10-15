using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.DTOs.FinancialGoals;

public class FinancialGoalUpdateDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal TargetAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal CurrentAmount { get; set; }

    public DateTime? TargetDate { get; set; }

    [Required]
    public GoalStatus Status { get; set; }
}