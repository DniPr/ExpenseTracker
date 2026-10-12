using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.Data.Models.Entities;

public class FinancialGoal
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal TargetAmount { get; set; }

    public decimal CurrentAmount { get; set; }

    public DateTime? TargetDate { get; set; }

    [Required]
    public GoalStatus Status { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
}