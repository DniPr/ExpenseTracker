using ExpenseTracker.DTOs.Transactions;

namespace ExpenseTracker.Services.Interfaces;

public interface ITransactionService
{
    Task<IEnumerable<TransactionDto>> GetAllAsync(TransactionFilterDto filter,string userId);

    Task<TransactionDto?> GetByIdAsync(int id,string userId);

    Task<TransactionDto?> CreateAsync(TransactionCreateDto dto,string userId);

    Task<bool> UpdateAsync(int id,TransactionUpdateDto dto,string userId);

    Task<bool> DeleteAsync(int id,string userId);
}