namespace ExpenseTracker.DTOs.Dashboard;

public class MonthlySummaryDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public decimal Income { get; set; }

    public decimal Expenses { get; set; }
}