using ExpenseTracker.DTOs.FinancialGoals;
using ExpenseTracker.DTOs.RecurringTransactions;
using ExpenseTracker.DTOs.Transactions;

namespace ExpenseTracker.DTOs.Dashboard;

public class DashboardDto
{
    public decimal CurrentBalance { get; set; }

    public decimal IncomeThisMonth { get; set; }

    public decimal ExpensesThisMonth { get; set; }

    public decimal SavingsThisMonth { get; set; }

    public IEnumerable<ExpenseByCategoryDto> ExpensesByCategory { get; set; }
        = new List<ExpenseByCategoryDto>();

    public IEnumerable<MonthlySummaryDto> MonthlySummary { get; set; }
        = new List<MonthlySummaryDto>();

    public IEnumerable<BudgetProgressDto> BudgetProgress { get; set; }
        = new List<BudgetProgressDto>();

    public IEnumerable<TransactionDto> RecentTransactions { get; set; }
        = new List<TransactionDto>();

    public IEnumerable<FinancialGoalDto> ActiveGoals { get; set; }
        = new List<FinancialGoalDto>();

    public IEnumerable<RecurringTransactionDto> UpcomingRecurringTransactions { get; set; }
        = new List<RecurringTransactionDto>();
}