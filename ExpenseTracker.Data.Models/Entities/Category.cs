using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.Data.Models.Entities;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(250)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    public bool IsDefault { get; set; }

    [ForeignKey(nameof(User))]
    public string? UserId { get; set; }

    public virtual ApplicationUser? User { get; set; }

    public DateTime CreatedOn { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; }
        = new HashSet<Transaction>();

    public virtual ICollection<Budget> Budgets { get; set; }
        = new HashSet<Budget>();

    public virtual ICollection<RecurringTransaction> RecurringTransactions { get; set; }
        = new HashSet<RecurringTransaction>();
}