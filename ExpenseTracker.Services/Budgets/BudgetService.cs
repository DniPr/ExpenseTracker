using ExpenseTracker.Data;
using ExpenseTracker.Data.Models.Entities;
using ExpenseTracker.Data.Models.Enums;
using ExpenseTracker.DTOs.Budgets;
using ExpenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services.Budgets;

public class BudgetService : IBudgetService
{
    private readonly ApplicationDbContext context;

    public BudgetService(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<BudgetDto>> GetAllAsync(string userId)
    {
        return await context.Budgets
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.Year)
            .ThenByDescending(b => b.Month)
            .Select(b => new BudgetDto
            {
                Id = b.Id,
                Amount = b.Amount,
                Period = b.Period,
                Month = b.Month,
                Year = b.Year,
                CategoryId = b.CategoryId,
                CategoryName = b.Category != null
                    ? b.Category.Name
                    : null,
                CreatedOn = b.CreatedOn,
                UpdatedOn = b.UpdatedOn
            })
            .ToListAsync();
    }

    public async Task<BudgetDto?> GetByIdAsync(int id, string userId)
    {
        return await context.Budgets
            .AsNoTracking()
            .Where(b =>
                b.Id == id &&
                b.UserId == userId)
            .Select(b => new BudgetDto
            {
                Id = b.Id,
                Amount = b.Amount,
                Period = b.Period,
                Month = b.Month,
                Year = b.Year,
                CategoryId = b.CategoryId,
                CategoryName = b.Category != null
                    ? b.Category.Name
                    : null,
                CreatedOn = b.CreatedOn,
                UpdatedOn = b.UpdatedOn
            })
            .FirstOrDefaultAsync();
    }

    public async Task<BudgetDto?> CreateAsync(BudgetCreateDto dto, string userId)
    {
        if (!IsPeriodValid(dto.Period, dto.Month))
        {
            return null;
        }

        if (!IsBudgetPeriodCurrentOrFuture(dto.Period, dto.Month, dto.Year))
        {
            return null;
        }

        if (dto.CategoryId.HasValue)
        {
            Category? category =
                await context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == dto.CategoryId.Value &&
                        (c.IsDefault ||
                         c.UserId == userId));

            if (category == null)
            {
                return null;
            }

            if (category.Type != TransactionType.Expense)
            {
                return null;
            }
        }

        bool duplicateExists =
            await context.Budgets.AnyAsync(b =>
                b.UserId == userId &&
                b.Period == dto.Period &&
                b.Month == dto.Month &&
                b.Year == dto.Year &&
                b.CategoryId == dto.CategoryId);

        if (duplicateExists)
        {
            return null;
        }

        Budget budget = new Budget
        {
            Amount = dto.Amount,
            Period = dto.Period,
            Month = dto.Month,
            Year = dto.Year,
            CategoryId = dto.CategoryId,
            UserId = userId,
            CreatedOn = DateTime.UtcNow
        };

        await context.Budgets.AddAsync(budget);
        await context.SaveChangesAsync();

        string? categoryName = null;

        if (budget.CategoryId.HasValue)
        {
            categoryName = await context.Categories
                .Where(c => c.Id == budget.CategoryId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();
        }

        return new BudgetDto
        {
            Id = budget.Id,
            Amount = budget.Amount,
            Period = budget.Period,
            Month = budget.Month,
            Year = budget.Year,
            CategoryId = budget.CategoryId,
            CategoryName = categoryName,
            CreatedOn = budget.CreatedOn,
            UpdatedOn = budget.UpdatedOn
        };
    }

    public async Task<bool> UpdateAsync(int id, BudgetUpdateDto dto, string userId)
    {
        Budget? budget =
            await context.Budgets
                .FirstOrDefaultAsync(b =>
                    b.Id == id &&
                    b.UserId == userId);

        if (budget == null)
        {
            return false;
        }

        if (!IsPeriodValid(dto.Period, dto.Month))
        {
            return false;
        }

        if (!IsBudgetPeriodCurrentOrFuture(dto.Period, dto.Month, dto.Year))
        {
            return false;
        }

        if (dto.CategoryId.HasValue)
        {
            Category? category =
                await context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c =>
                        c.Id == dto.CategoryId.Value &&
                        (c.IsDefault ||
                         c.UserId == userId));

            if (category == null)
            {
                return false;
            }

            if (category.Type != TransactionType.Expense)
            {
                return false;
            }
        }

        bool duplicateExists =
            await context.Budgets.AnyAsync(b =>
                b.Id != id &&
                b.UserId == userId &&
                b.Period == dto.Period &&
                b.Month == dto.Month &&
                b.Year == dto.Year &&
                b.CategoryId == dto.CategoryId);

        if (duplicateExists)
        {
            return false;
        }

        budget.Amount = dto.Amount;
        budget.Period = dto.Period;
        budget.Month = dto.Month;
        budget.Year = dto.Year;
        budget.CategoryId = dto.CategoryId;
        budget.UpdatedOn = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        Budget? budget =
            await context.Budgets
                .FirstOrDefaultAsync(b =>
                    b.Id == id &&
                    b.UserId == userId);

        if (budget == null)
        {
            return false;
        }

        context.Budgets.Remove(budget);
        await context.SaveChangesAsync();

        return true;
    }

    private static bool IsPeriodValid(BudgetPeriod period, int? month)
    {
        if (period == BudgetPeriod.Monthly)
        {
            return month.HasValue;
        }

        if (period == BudgetPeriod.Yearly)
        {
            return month == null;
        }

        return false;
    }

    private static bool IsBudgetPeriodCurrentOrFuture(BudgetPeriod period, int? month, int year)
    {
        DateTime now = DateTime.UtcNow;

        if (period == BudgetPeriod.Monthly)
        {
            if (!month.HasValue)
            {
                return false;
            }

            DateTime budgetMonth =
                new DateTime(year, month.Value, 1);

            DateTime currentMonth =
                new DateTime(now.Year, now.Month, 1);

            return budgetMonth >= currentMonth;
        }

        if (period == BudgetPeriod.Yearly)
        {
            return year >= now.Year;
        }

        return false;
    }
}