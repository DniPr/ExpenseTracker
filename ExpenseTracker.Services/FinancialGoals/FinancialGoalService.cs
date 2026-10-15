using ExpenseTracker.Data;
using ExpenseTracker.Data.Models.Entities;
using ExpenseTracker.Data.Models.Enums;
using ExpenseTracker.DTOs.FinancialGoals;
using ExpenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services.FinancialGoals;

public class FinancialGoalService : IFinancialGoalService
{
    private readonly ApplicationDbContext context;

    public FinancialGoalService(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<FinancialGoalDto>> GetAllAsync(string userId)
    {
        return await context.FinancialGoals
            .AsNoTracking()
            .Where(g => g.UserId == userId)
            .OrderBy(g => g.Status)
            .ThenBy(g => g.TargetDate)
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
    }

    public async Task<FinancialGoalDto?> GetByIdAsync(int id,string userId)
    {
        return await context.FinancialGoals
            .AsNoTracking()
            .Where(g =>
                g.Id == id &&
                g.UserId == userId)
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
            .FirstOrDefaultAsync();
    }

    public async Task<FinancialGoalDto?> CreateAsync(FinancialGoalCreateDto dto,string userId)
    {
        if (!IsGoalValid(dto.TargetAmount,dto.CurrentAmount,dto.TargetDate))
        {
            return null;
        }

        GoalStatus status =
            dto.CurrentAmount >= dto.TargetAmount
                ? GoalStatus.Completed
                : GoalStatus.Active;

        FinancialGoal goal = new FinancialGoal
        {
            Name = dto.Name,
            Description = dto.Description,
            TargetAmount = dto.TargetAmount,
            CurrentAmount = dto.CurrentAmount,
            TargetDate = dto.TargetDate,
            Status = status,
            UserId = userId,
            CreatedOn = DateTime.UtcNow
        };

        await context.FinancialGoals.AddAsync(goal);
        await context.SaveChangesAsync();

        return new FinancialGoalDto
        {
            Id = goal.Id,
            Name = goal.Name,
            Description = goal.Description,
            TargetAmount = goal.TargetAmount,
            CurrentAmount = goal.CurrentAmount,
            TargetDate = goal.TargetDate,
            Status = goal.Status,
            CreatedOn = goal.CreatedOn,
            UpdatedOn = goal.UpdatedOn
        };
    }

    public async Task<bool> UpdateAsync(int id,FinancialGoalUpdateDto dto,string userId)
    {
        FinancialGoal? goal =
            await context.FinancialGoals
                .FirstOrDefaultAsync(g =>
                    g.Id == id &&
                    g.UserId == userId);

        if (goal == null)
        {
            return false;
        }

        if (!IsGoalValid(dto.TargetAmount,dto.CurrentAmount,dto.TargetDate))
        {
            return false;
        }

        GoalStatus status = dto.Status;

        if (dto.CurrentAmount >= dto.TargetAmount)
        {
            status = GoalStatus.Completed;
        }
        else if (status == GoalStatus.Completed)
        {
            status = GoalStatus.Active;
        }

        goal.Name = dto.Name;
        goal.Description = dto.Description;
        goal.TargetAmount = dto.TargetAmount;
        goal.CurrentAmount = dto.CurrentAmount;
        goal.TargetDate = dto.TargetDate;
        goal.Status = status;
        goal.UpdatedOn = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id,string userId)
    {
        FinancialGoal? goal =
            await context.FinancialGoals
                .FirstOrDefaultAsync(g =>
                    g.Id == id &&
                    g.UserId == userId);

        if (goal == null)
        {
            return false;
        }

        context.FinancialGoals.Remove(goal);
        await context.SaveChangesAsync();

        return true;
    }

    private static bool IsGoalValid(decimal targetAmount,decimal currentAmount,DateTime? targetDate)
    {
        if (targetAmount <= 0)
        {
            return false;
        }

        if (currentAmount < 0)
        {
            return false;
        }

        if (targetDate.HasValue &&
            targetDate.Value.Date < DateTime.UtcNow.Date)
        {
            return false;
        }

        return true;
    }
}