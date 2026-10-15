namespace ExpenseTracker.DTOs.Dashboard;

public class BudgetProgressDto
{
    public int BudgetId { get; set; }

    public decimal BudgetAmount { get; set; }

    public decimal SpentAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public double PercentageUsed { get; set; }

    public int? CategoryId { get; set; }

    public string? CategoryName { get; set; }
}