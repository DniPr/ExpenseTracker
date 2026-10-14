using ExpenseTracker.Data;
using ExpenseTracker.Data.Models.Entities;
using ExpenseTracker.DTOs.Categories;
using ExpenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services.Categories;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext context;

    public CategoryService(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(string userId)
    {
        return await context.Categories
            .AsNoTracking()
            .Where(c =>
                c.IsDefault ||
                c.UserId == userId)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Icon = c.Icon,
                Type = c.Type,
                IsDefault = c.IsDefault
            })
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(int id,string userId)
    {
        return await context.Categories
            .AsNoTracking()
            .Where(c =>
                c.Id == id &&
                (c.IsDefault || c.UserId == userId))
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Icon = c.Icon,
                Type = c.Type,
                IsDefault = c.IsDefault
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryDto?> CreateAsync(CategoryCreateDto dto,string userId)
    {
        bool alreadyExists = await context.Categories
            .AnyAsync(c =>
                c.UserId == userId &&
                c.Name == dto.Name &&
                c.Type == dto.Type);

        if (alreadyExists)
        {
            return null;
        }

        Category category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            Icon = dto.Icon,
            Type = dto.Type,
            IsDefault = false,
            UserId = userId,
            CreatedOn = DateTime.UtcNow
        };

        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Icon = category.Icon,
            Type = category.Type,
            IsDefault = category.IsDefault
        };
    }

    public async Task<bool> UpdateAsync(int id,CategoryUpdateDto dto,string userId)
    {
        Category? category =
            await context.Categories
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.UserId == userId &&
                    !c.IsDefault);

        if (category == null)
        {
            return false;
        }

        bool duplicateExists = await context.Categories
            .AnyAsync(c =>
                c.Id != id &&
                c.UserId == userId &&
                c.Name == dto.Name &&
                c.Type == dto.Type);

        if (duplicateExists)
        {
            return false;
        }

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.Icon = dto.Icon;
        category.Type = dto.Type;

        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id,string userId)
    {
        Category? category =
            await context.Categories
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.UserId == userId &&
                    !c.IsDefault);

        if (category == null)
        {
            return false;
        }

        bool isInUse =
            await context.Transactions
                .AnyAsync(t => t.CategoryId == id)
            ||
            await context.Budgets
                .AnyAsync(b => b.CategoryId == id)
            ||
            await context.RecurringTransactions
                .AnyAsync(rt => rt.CategoryId == id);

        if (isInUse)
        {
            return false;
        }

        context.Categories.Remove(category);
        await context.SaveChangesAsync();

        return true;
    }
}