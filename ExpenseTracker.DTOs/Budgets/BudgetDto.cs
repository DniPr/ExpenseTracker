using ExpenseTracker.Data.Models.Enums;

namespace ExpenseTracker.DTOs.Budgets;

public class BudgetDto
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public BudgetPeriod Period { get; set; }

    public int? Month { get; set; }

    public int Year { get; set; }

    public int? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }
}