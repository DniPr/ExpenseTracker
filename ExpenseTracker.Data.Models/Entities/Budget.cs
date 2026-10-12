using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.Data.Models.Entities;

public class Budget
{
    [Key]
    public int Id { get; set; }

    public decimal Amount { get; set; }

    [Required]
    public BudgetPeriod Period { get; set; }

    public int? Month { get; set; }

    public int Year { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    [ForeignKey(nameof(Category))]
    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
}