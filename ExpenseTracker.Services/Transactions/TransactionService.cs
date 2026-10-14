using ExpenseTracker.Data;
using ExpenseTracker.Data.Models.Entities;
using ExpenseTracker.DTOs.Transactions;
using ExpenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services.Transactions;

public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext context;

    public TransactionService(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<TransactionDto>> GetAllAsync(TransactionFilterDto filter,string userId)
    {
        IQueryable<Transaction> query = context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        if (filter.FromDate.HasValue)
        {
            query = query.Where(t =>
                t.Date >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(t =>
                t.Date <= filter.ToDate.Value);
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(t =>
                t.Type == filter.Type.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(t =>
                t.CategoryId == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            string searchTerm =
                filter.SearchTerm.Trim().ToLower();

            query = query.Where(t =>
                t.Description.ToLower().Contains(searchTerm) ||
                (t.Notes != null &&
                 t.Notes.ToLower().Contains(searchTerm)));
        }

        return await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedOn)
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
    }

    public async Task<TransactionDto?> GetByIdAsync(int id,string userId)
    {
        return await context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.Id == id &&
                t.UserId == userId)
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
            .FirstOrDefaultAsync();
    }

    public async Task<TransactionDto?> CreateAsync(TransactionCreateDto dto,string userId)
    {
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

        Transaction transaction = new Transaction
        {
            Description = dto.Description,
            Amount = dto.Amount,
            Date = dto.Date,
            Type = dto.Type,
            Notes = dto.Notes,
            CategoryId = dto.CategoryId,
            UserId = userId,
            CreatedOn = DateTime.UtcNow
        };

        await context.Transactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        return new TransactionDto
        {
            Id = transaction.Id,
            Description = transaction.Description,
            Amount = transaction.Amount,
            Date = transaction.Date,
            Type = transaction.Type,
            Notes = transaction.Notes,
            CategoryId = transaction.CategoryId,
            CategoryName = category.Name,
            CreatedOn = transaction.CreatedOn,
            UpdatedOn = transaction.UpdatedOn
        };
    }

    public async Task<bool> UpdateAsync(int id,TransactionUpdateDto dto,string userId)
    {
        Transaction? transaction =
            await context.Transactions
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == userId);

        if (transaction == null)
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

        transaction.Description = dto.Description;
        transaction.Amount = dto.Amount;
        transaction.Date = dto.Date;
        transaction.Type = dto.Type;
        transaction.Notes = dto.Notes;
        transaction.CategoryId = dto.CategoryId;
        transaction.UpdatedOn = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id,string userId)
    {
        Transaction? transaction =
            await context.Transactions
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.UserId == userId);

        if (transaction == null)
        {
            return false;
        }

        context.Transactions.Remove(transaction);
        await context.SaveChangesAsync();

        return true;
    }
}