using ExpenseTracker.Data;
using ExpenseTracker.Data.Models.Enums;
using ExpenseTracker.DTOs.Dashboard;
using ExpenseTracker.DTOs.FinancialGoals;
using ExpenseTracker.DTOs.RecurringTransactions;
using ExpenseTracker.DTOs.Transactions;
using ExpenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext context;

    public DashboardService(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<DashboardDto> GetDashboardAsync(string userId)
    {
        DateTime now = DateTime.UtcNow;

        DateTime currentMonthStart =
            new DateTime(now.Year, now.Month, 1);

        DateTime nextMonthStart =
            currentMonthStart.AddMonths(1);

        decimal totalIncome =
            await context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.UserId == userId &&
                    t.Type == TransactionType.Income)
                .SumAsync(t => (decimal?)t.Amount)
                ?? 0;

        decimal totalExpenses =
            await context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.UserId == userId &&
                    t.Type == TransactionType.Expense)
                .SumAsync(t => (decimal?)t.Amount)
                ?? 0;

        decimal incomeThisMonth =
            await context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.UserId == userId &&
                    t.Type == TransactionType.Income &&
                    t.Date >= currentMonthStart &&
                    t.Date < nextMonthStart)
                .SumAsync(t => (decimal?)t.Amount)
                ?? 0;

        decimal expensesThisMonth =
            await context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.UserId == userId &&
                    t.Type == TransactionType.Expense &&
                    t.Date >= currentMonthStart &&
                    t.Date < nextMonthStart)
                .SumAsync(t => (decimal?)t.Amount)
                ?? 0;

        List<ExpenseByCategoryDto> expensesByCategory =
            await context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.UserId == userId &&
                    t.Type == TransactionType.Expense &&
                    t.Date >= currentMonthStart &&
                    t.Date < nextMonthStart)
                .GroupBy(t => new
                {
                    t.CategoryId,
                    t.Category.Name
                })
                .Select(g => new ExpenseByCategoryDto
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    Amount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(x => x.Amount)
                .ToListAsync();

        DateTime sixMonthsAgo =
            currentMonthStart.AddMonths(-5);

        var monthlyData =
            await context.Transactions
                .AsNoTracking()
                .Where(t =>
                    t.UserId == userId &&
                    t.Date >= sixMonthsAgo &&
                    t.Date < nextMonthStart)
                .GroupBy(t => new
                {
                    t.Date.Year,
                    t.Date.Month,
                    t.Type
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Type,
                    Amount = g.Sum(t => t.Amount)
                })
                .ToListAsync();

        List<MonthlySummaryDto> monthlySummary = new List<MonthlySummaryDto>();

        for (int i = 0; i < 6; i++)
        {
            DateTime month =
                sixMonthsAgo.AddMonths(i);

            decimal income = monthlyData
                .Where(x =>
                    x.Year == month.Year &&
                    x.Month == month.Month &&
                    x.Type == TransactionType.Income)
                .Sum(x => x.Amount);

            decimal expenses = monthlyData
                .Where(x =>
                    x.Year == month.Year &&
                    x.Month == month.Month &&
                    x.Type == TransactionType.Expense)
                .Sum(x => x.Amount);

            monthlySummary.Add(new MonthlySummaryDto
            {
                Year = month.Year,
                Month = month.Month,
                Income = income,
                Expenses = expenses
            });
        }

        List<BudgetProgressDto> budgetProgress =
            await GetBudgetProgressAsync(
                userId,
                now.Year,
                now.Month,
                currentMonthStart,
                nextMonthStart);

        List<TransactionDto> recentTransactions =
            await context.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.CreatedOn)
                .Take(5)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Description = t.Description,
                    Amount = t.Amount,
                    Date = t.Date,
                    Type = t.Type,
                    Notes = t.Notes,
                    CategoryId = t.CategoryId,
                    CategoryName = t.Category.Name,
                    CreatedOn = t.CreatedOn,
                    UpdatedOn = t.UpdatedOn
                })
                .ToListAsync();

        List<FinancialGoalDto> activeGoals =
            await context.FinancialGoals
                .AsNoTracking()
                .Where(g =>
                    g.UserId == userId &&
                    g.Status == GoalStatus.Active)
                .OrderBy(g => g.TargetDate)
                .Take(5)
                .Select(g => new FinancialGoalDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    TargetAmount = g.TargetAmount,
                    CurrentAmount = g.CurrentAmount,
                    TargetDate = g.TargetDate,
                    Status = g.Status,
                    CreatedOn = g.CreatedOn,
                    UpdatedOn = g.UpdatedOn
                })
                .ToListAsync();

        List<RecurringTransactionDto> upcomingRecurringTransactions =
            await context.RecurringTransactions
                .AsNoTracking()
                .Where(rt =>
                    rt.UserId == userId &&
                    rt.IsActive &&
                    (!rt.EndDate.HasValue ||
                     rt.EndDate.Value >= now))
                .OrderBy(rt => rt.NextExecutionDate)
                .Take(5)
                .Select(rt => new RecurringTransactionDto
                {
                    Id = rt.Id,
                    Description = rt.Description,
                    Amount = rt.Amount,
                    Type = rt.Type,
                    StartDate = rt.StartDate,
                    NextExecutionDate = rt.NextExecutionDate,
                    EndDate = rt.EndDate,
                    Interval = rt.Interval,
                    IsActive = rt.IsActive,
                    CategoryId = rt.CategoryId,
                    CategoryName = rt.Category.Name,
                    CreatedOn = rt.CreatedOn
                })
                .ToListAsync();

        return new DashboardDto
        {
            CurrentBalance =
                totalIncome - totalExpenses,

            IncomeThisMonth =
                incomeThisMonth,

            ExpensesThisMonth =
                expensesThisMonth,

            SavingsThisMonth =
                incomeThisMonth - expensesThisMonth,

            ExpensesByCategory =
                expensesByCategory,

            MonthlySummary =
                monthlySummary,

            BudgetProgress =
                budgetProgress,

            RecentTransactions =
                recentTransactions,

            ActiveGoals =
                activeGoals,

            UpcomingRecurringTransactions =
                upcomingRecurringTransactions
        };
    }

    private async Task<List<BudgetProgressDto>>
        GetBudgetProgressAsync(
            string userId,
            int year,
            int month,
            DateTime monthStart,
            DateTime nextMonthStart)
    {
        var budgets = await context.Budgets
            .AsNoTracking()
            .Where(b =>
                b.UserId == userId &&
                (
                    (b.Period == BudgetPeriod.Monthly &&
                     b.Year == year &&
                     b.Month == month)
                    ||
                    (b.Period == BudgetPeriod.Yearly &&
                     b.Year == year)
                ))
            .Select(b => new
            {
                b.Id,
                b.Amount,
                b.Period,
                b.CategoryId,
                CategoryName =
                    b.Category != null
                        ? b.Category.Name
                        : null
            })
            .ToListAsync();

        List<BudgetProgressDto> result =
            new List<BudgetProgressDto>();

        foreach (var budget in budgets)
        {
            IQueryable<Data.Models.Entities.Transaction>
                transactions = context.Transactions
                    .AsNoTracking()
                    .Where(t =>
                        t.UserId == userId &&
                        t.Type == TransactionType.Expense);

            if (budget.Period == BudgetPeriod.Monthly)
            {
                transactions = transactions.Where(t =>
                    t.Date >= monthStart &&
                    t.Date < nextMonthStart);
            }
            else
            {
                DateTime yearStart =
                    new DateTime(year, 1, 1);

                DateTime nextYearStart =
                    yearStart.AddYears(1);

                transactions = transactions.Where(t =>
                    t.Date >= yearStart &&
                    t.Date < nextYearStart);
            }

            if (budget.CategoryId.HasValue)
            {
                transactions = transactions.Where(t =>
                    t.CategoryId == budget.CategoryId.Value);
            }

            decimal spent =
                await transactions
                    .SumAsync(t => (decimal?)t.Amount)
                ?? 0;

            decimal remaining =
                budget.Amount - spent;

            double percentageUsed =
                budget.Amount > 0
                    ? (double)(spent / budget.Amount) * 100
                    : 0;

            result.Add(new BudgetProgressDto
            {
                BudgetId = budget.Id,
                BudgetAmount = budget.Amount,
                SpentAmount = spent,
                RemainingAmount = remaining,
                PercentageUsed = percentageUsed,
                CategoryId = budget.CategoryId,
                CategoryName = budget.CategoryName
            });
        }

        return result;
    }
}