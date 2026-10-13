using ExpenseTracker.Data.Models.Entities;
using ExpenseTracker.Data.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data.Seeding;

public static class CategorySeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        bool hasDefaultCategories = await context.Categories
            .AnyAsync(c => c.IsDefault);

        if (hasDefaultCategories)
        {
            return;
        }

        List<Category> categories = new()
        {
            new Category
            {
                Name = "Salary",
                Description = "Income from salary or wages",
                Icon = "wallet",
                Type = TransactionType.Income,
                IsDefault = true,
                UserId = null,
                CreatedOn = DateTime.UtcNow
            },
            new Category
            {
                Name = "Freelance",
                Description = "Income from freelance work",
                Icon = "briefcase",
                Type = TransactionType.Income,
                IsDefault = true,
                UserId = null,
                CreatedOn = DateTime.UtcNow
            },
            new Category
            {
                Name = "Other Income",
                Description = "Other sources of income",
                Icon = "circle-plus",
                Type = TransactionType.Income,
                IsDefault = true,
                UserId = null,
                CreatedOn = DateTime.UtcNow
            },

            new Category
            {
                Name = "Food",
                Description = "Groceries, restaurants and food",
                Icon = "utensils",
                Type = TransactionType.Expense,
                IsDefault = true,
                UserId = null,
                CreatedOn = DateTime.UtcNow
            },
            new Category
            {
                Name = "Transport",
                Description = "Fuel, public transport and travel",
                Icon = "car",
                Type = TransactionType.Expense,
                IsDefault = true,
                UserId = null,
                CreatedOn = DateTime.UtcNow
            },
            new Category
            {
                Name = "Bills",
                Description = "Utilities and recurring household bills",
                Icon = "file-invoice",
                Type = TransactionType.Expense,
                IsDefault = true,
                UserId = null,
                CreatedOn = DateTime.UtcNow
            },
            new Category
            {
                Name = "Entertainment",
                Description = "Movies, games and entertainment",
                Icon = "gamepad",
                Type = TransactionType.Expense,
                IsDefault = true,
                UserId = null,
                CreatedOn = DateTime.UtcNow
            },
            new Category
            {
                Name = "Health",
                Description = "Healthcare and medicine",
                Icon = "heart-pulse",
                Type = TransactionType.Expense,
                IsDefault = true,
                UserId = null,
                CreatedOn = DateTime.UtcNow
            },
            new Category
            {
                Name = "Shopping",
                Description = "Personal shopping and purchases",
                Icon = "bag-shopping",
                Type = TransactionType.Expense,
                IsDefault = true,
                UserId = null,
                CreatedOn = DateTime.UtcNow
            },
            new Category
            {
                Name = "Other Expense",
                Description = "Other expenses",
                Icon = "ellipsis",
                Type = TransactionType.Expense,
                IsDefault = true,
                UserId = null,
                CreatedOn = DateTime.UtcNow
            }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }
}