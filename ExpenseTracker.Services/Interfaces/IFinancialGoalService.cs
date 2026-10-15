using ExpenseTracker.DTOs.FinancialGoals;

namespace ExpenseTracker.Services.Interfaces;

public interface IFinancialGoalService
{
    Task<IEnumerable<FinancialGoalDto>> GetAllAsync(string userId);

    Task<FinancialGoalDto?> GetByIdAsync(int id,string userId);

    Task<FinancialGoalDto?> CreateAsync(FinancialGoalCreateDto dto,string userId);

    Task<bool> UpdateAsync(int id,FinancialGoalUpdateDto dto,string userId);

    Task<bool> DeleteAsync(int id,string userId);
}