using ExpenseTracker.DTOs.Budgets;

namespace ExpenseTracker.Services.Interfaces;

public interface IBudgetService
{
    Task<IEnumerable<BudgetDto>> GetAllAsync(string userId);

    Task<BudgetDto?> GetByIdAsync(int id,string userId);

    Task<BudgetDto?> CreateAsync(BudgetCreateDto dto,string userId);

    Task<bool> UpdateAsync(int id,BudgetUpdateDto dto,string userId);

    Task<bool> DeleteAsync(int id,string userId);
}