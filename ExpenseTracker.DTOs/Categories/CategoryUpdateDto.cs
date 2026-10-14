using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.DTOs.Categories;

public class CategoryUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(250)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Icon { get; set; }

    [Required]
    public TransactionType Type { get; set; }
}