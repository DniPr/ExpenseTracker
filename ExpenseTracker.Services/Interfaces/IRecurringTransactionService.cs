using ExpenseTracker.DTOs.RecurringTransactions;

namespace ExpenseTracker.Services.Interfaces;

public interface IRecurringTransactionService
{
    Task<IEnumerable<RecurringTransactionDto>> GetAllAsync(string userId);

    Task<RecurringTransactionDto?> GetByIdAsync(int id,string userId);

    Task<RecurringTransactionDto?> CreateAsync(RecurringTransactionCreateDto dto,string userId);

    Task<bool> UpdateAsync(int id,RecurringTransactionUpdateDto dto,string userId);

    Task<bool> DeleteAsync(int id,string userId);
}