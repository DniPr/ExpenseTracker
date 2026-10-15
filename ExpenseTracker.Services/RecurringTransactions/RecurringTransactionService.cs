using ExpenseTracker.Data;
using ExpenseTracker.Data.Models.Entities;
using ExpenseTracker.Data.Models.Enums;
using ExpenseTracker.DTOs.RecurringTransactions;
using ExpenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services.RecurringTransactions;

public class RecurringTransactionService : IRecurringTransactionService
{
    private readonly ApplicationDbContext context;

    public RecurringTransactionService(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<RecurringTransactionDto>> GetAllAsync(string userId)
    {
        return await context.RecurringTransactions
            .AsNoTracking()
            .Where(rt => rt.UserId == userId)
            .OrderBy(rt => rt.NextExecutionDate)
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
    }

    public async Task<RecurringTransactionDto?> GetByIdAsync(int id,string userId)
    {
        return await context.RecurringTransactions
            .AsNoTracking()
            .Where(rt =>
                rt.Id == id &&
                rt.UserId == userId)
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
            .FirstOrDefaultAsync();
    }

    public async Task<RecurringTransactionDto?> CreateAsync(RecurringTransactionCreateDto dto,string userId)
    {
        if (!IsDateRangeValid(dto.StartDate,dto.EndDate))
        {
            return null;
        }

        Category? category = await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.Id == dto.CategoryId &&
                (c.IsDefault || c.UserId == userId));

        if (category == null)
        {
            return null;
        }

        if (category.Type != dto.Type)
        {
            return null;
        }

        DateTime nextExecutionDate =
            CalculateNextExecutionDate(dto.StartDate,dto.Interval);

        if (dto.EndDate.HasValue && nextExecutionDate > dto.EndDate.Value)
        {
            return null;
        }

        RecurringTransaction recurringTransaction = new RecurringTransaction
            {
                Description = dto.Description,
                Amount = dto.Amount,
                Type = dto.Type,
                StartDate = dto.StartDate,
                NextExecutionDate = nextExecutionDate,
                EndDate = dto.EndDate,
                Interval = dto.Interval,
                IsActive = true,
                CategoryId = dto.CategoryId,
                UserId = userId,
                CreatedOn = DateTime.UtcNow
            };

        await context.RecurringTransactions.AddAsync(recurringTransaction);

        await context.SaveChangesAsync();

        return new RecurringTransactionDto
        {
            Id = recurringTransaction.Id,
            Description = recurringTransaction.Description,
            Amount = recurringTransaction.Amount,
            Type = recurringTransaction.Type,
            StartDate = recurringTransaction.StartDate,
            NextExecutionDate =
                recurringTransaction.NextExecutionDate,
            EndDate = recurringTransaction.EndDate,
            Interval = recurringTransaction.Interval,
            IsActive = recurringTransaction.IsActive,
            CategoryId = recurringTransaction.CategoryId,
            CategoryName = category.Name,
            CreatedOn = recurringTransaction.CreatedOn
        };
    }

    public async Task<bool> UpdateAsync(int id,RecurringTransactionUpdateDto dto,string userId)
    {
        RecurringTransaction? recurringTransaction =
            await context.RecurringTransactions
                .FirstOrDefaultAsync(rt =>
                    rt.Id == id &&
                    rt.UserId == userId);

        if (recurringTransaction == null)
        {
            return false;
        }

        if (!IsDateRangeValid(dto.StartDate,dto.EndDate))
        {
            return false;
        }

        Category? category = await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.Id == dto.CategoryId &&
                (c.IsDefault || c.UserId == userId));

        if (category == null)
        {
            return false;
        }

        if (category.Type != dto.Type)
        {
            return false;
        }

        DateTime nextExecutionDate =
            CalculateNextExecutionDate(dto.StartDate,dto.Interval);

        if (dto.EndDate.HasValue &&
            nextExecutionDate > dto.EndDate.Value)
        {
            return false;
        }

        recurringTransaction.Description =
            dto.Description;

        recurringTransaction.Amount =
            dto.Amount;

        recurringTransaction.Type =
            dto.Type;

        recurringTransaction.StartDate =
            dto.StartDate;

        recurringTransaction.NextExecutionDate =
            nextExecutionDate;

        recurringTransaction.EndDate =
            dto.EndDate;

        recurringTransaction.Interval =
            dto.Interval;

        recurringTransaction.IsActive =
            dto.IsActive;

        recurringTransaction.CategoryId =
            dto.CategoryId;

        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id,string userId)
    {
        RecurringTransaction? recurringTransaction =
            await context.RecurringTransactions
                .FirstOrDefaultAsync(rt =>
                    rt.Id == id &&
                    rt.UserId == userId);

        if (recurringTransaction == null)
        {
            return false;
        }

        context.RecurringTransactions.Remove(recurringTransaction);

        await context.SaveChangesAsync();

        return true;
    }

    private static bool IsDateRangeValid(DateTime startDate,DateTime? endDate)
    {
        if (!endDate.HasValue)
        {
            return true;
        }

        return endDate.Value >= startDate;
    }

    private static DateTime CalculateNextExecutionDate(DateTime startDate,RecurringInterval interval)
    {
        return interval switch
        {
            RecurringInterval.Weekly =>
                startDate.AddDays(7),

            RecurringInterval.Monthly =>
                startDate.AddMonths(1),

            RecurringInterval.Yearly =>
                startDate.AddYears(1),

            _ => throw new ArgumentOutOfRangeException(nameof(interval))
        };
    }
}