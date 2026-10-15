namespace ExpenseTracker.DTOs.Dashboard;

public class ExpenseByCategoryDto
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public decimal Amount { get; set; }
}