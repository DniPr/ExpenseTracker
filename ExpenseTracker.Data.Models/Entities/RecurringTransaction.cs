using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.Data.Models.Entities;

public class RecurringTransaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Description { get; set; } = null!;

    public decimal Amount { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime NextExecutionDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public RecurringInterval Interval { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedOn { get; set; }

    [ForeignKey(nameof(Category))]
    public int CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
}