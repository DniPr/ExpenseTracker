using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.Data.Models.Entities;

public class Transaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Description { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public TransactionType Type { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    [ForeignKey(nameof(Category))]
    public int CategoryId { get; set; }

    public virtual Category Category { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
}