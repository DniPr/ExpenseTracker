using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.DTOs.Categories;

public class CategoryDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Icon { get; set; }

    public TransactionType Type { get; set; }

    public bool IsDefault { get; set; }
}