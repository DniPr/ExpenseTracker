using ExpenseTracker.Data;
using ExpenseTracker.Data.Models.Entities;
using ExpenseTracker.Data.Models.Enums;
using ExpenseTracker.DTOs.RecurringTransactions;
using ExpenseTracker.Services.RecurringTransactions;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Tests;

[TestFixture]
public class RecurringTransactionServiceTests
{
    private ApplicationDbContext context = null!;
    private RecurringTransactionService service = null!;

    private const string UserId = "user-1";
    private const string OtherUserId = "user-2";

    [SetUp]
    public async Task SetUp()
    {
        DbContextOptions<ApplicationDbContext> options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"RecurringTransactionServiceTests_{Guid.NewGuid()}")
                .Options;

        context = new ApplicationDbContext(options);
        service = new RecurringTransactionService(context);

        await SeedDataAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnOnlyRecurringTransactionsForCurrentUser()
    {
        IEnumerable<RecurringTransactionDto> result = await service.GetAllAsync(UserId);

        List<RecurringTransactionDto> recurringTransactions = result.ToList();

        Assert.That(recurringTransactions, Has.Count.EqualTo(2));
        Assert.That(recurringTransactions.All(rt => rt.Id != 303), Is.True);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnRecurringTransaction_WhenOwnedByUser()
    {
        RecurringTransactionDto? result = await service.GetByIdAsync(301, UserId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Description, Is.EqualTo("Weekly Groceries"));
        Assert.That(result.CategoryName, Is.EqualTo("Food"));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnNull_WhenRecurringTransactionBelongsToAnotherUser()
    {
        RecurringTransactionDto? result = await service.GetByIdAsync(303, UserId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_ShouldCreateRecurringTransaction_WhenDataIsValid()
    {
        RecurringTransactionCreateDto dto = new RecurringTransactionCreateDto
        {
            Description = "Monthly Gym",
            Amount = 50m,
            Type = TransactionType.Expense,
            StartDate = new DateTime(2026, 10, 20),
            EndDate = new DateTime(2027, 10, 20),
            Interval = RecurringInterval.Monthly,
            CategoryId = 3
        };

        RecurringTransactionDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Description, Is.EqualTo("Monthly Gym"));
        Assert.That(result.CategoryName, Is.EqualTo("Gym"));
        Assert.That(result.IsActive, Is.True);
        Assert.That(result.NextExecutionDate, Is.EqualTo(new DateTime(2026, 11, 20)));

        RecurringTransaction? entity = await context.RecurringTransactions.FirstOrDefaultAsync(rt => rt.Id == result.Id);

        Assert.That(entity, Is.Not.Null);
        Assert.That(entity!.UserId, Is.EqualTo(UserId));
    }

    [Test]
    public async Task CreateAsync_ShouldCalculateWeeklyNextExecutionDate()
    {
        RecurringTransactionCreateDto dto = new RecurringTransactionCreateDto
        {
            Description = "Weekly Food",
            Amount = 25m,
            Type = TransactionType.Expense,
            StartDate = new DateTime(2026, 10, 20),
            EndDate = null,
            Interval = RecurringInterval.Weekly,
            CategoryId = 2
        };

        RecurringTransactionDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.NextExecutionDate, Is.EqualTo(new DateTime(2026, 10, 27)));
    }

    [Test]
    public async Task CreateAsync_ShouldReturnNull_WhenEndDateIsBeforeStartDate()
    {
        RecurringTransactionCreateDto dto = new RecurringTransactionCreateDto
        {
            Description = "Invalid Dates",
            Amount = 25m,
            Type = TransactionType.Expense,
            StartDate = new DateTime(2026, 10, 20),
            EndDate = new DateTime(2026, 10, 19),
            Interval = RecurringInterval.Weekly,
            CategoryId = 2
        };

        RecurringTransactionDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_ShouldReturnNull_WhenCategoryTypeDoesNotMatch()
    {
        RecurringTransactionCreateDto dto = new RecurringTransactionCreateDto
        {
            Description = "Wrong Category Type",
            Amount = 100m,
            Type = TransactionType.Income,
            StartDate = new DateTime(2026, 10, 20),
            EndDate = null,
            Interval = RecurringInterval.Monthly,
            CategoryId = 2
        };

        RecurringTransactionDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_ShouldReturnNull_WhenNextExecutionDateIsAfterEndDate()
    {
        RecurringTransactionCreateDto dto = new RecurringTransactionCreateDto
        {
            Description = "Too Short",
            Amount = 25m,
            Type = TransactionType.Expense,
            StartDate = new DateTime(2026, 10, 20),
            EndDate = new DateTime(2026, 10, 25),
            Interval = RecurringInterval.Weekly,
            CategoryId = 2
        };

        RecurringTransactionDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateRecurringTransaction_WhenDataIsValid()
    {
        RecurringTransactionUpdateDto dto = new RecurringTransactionUpdateDto
        {
            Description = "Updated Subscription",
            Amount = 30m,
            Type = TransactionType.Expense,
            StartDate = new DateTime(2026, 11, 1),
            EndDate = new DateTime(2027, 11, 1),
            Interval = RecurringInterval.Monthly,
            IsActive = false,
            CategoryId = 2
        };

        bool result = await service.UpdateAsync(302, dto, UserId);

        RecurringTransaction recurringTransaction = await context.RecurringTransactions.FirstAsync(rt => rt.Id == 302);

        Assert.That(result, Is.True);
        Assert.That(recurringTransaction.Description, Is.EqualTo("Updated Subscription"));
        Assert.That(recurringTransaction.Amount, Is.EqualTo(30m));
        Assert.That(recurringTransaction.IsActive, Is.False);
        Assert.That(recurringTransaction.NextExecutionDate, Is.EqualTo(new DateTime(2026, 12, 1)));
    }

    [Test]
    public async Task UpdateAsync_ShouldReturnFalse_WhenRecurringTransactionBelongsToAnotherUser()
    {
        RecurringTransactionUpdateDto dto = CreateValidUpdateDto();

        bool result = await service.UpdateAsync(303, dto, UserId);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteAsync_ShouldDeleteRecurringTransaction_WhenOwnedByUser()
    {
        bool result = await service.DeleteAsync(302, UserId);

        RecurringTransaction? recurringTransaction = await context.RecurringTransactions.FirstOrDefaultAsync(rt => rt.Id == 302);

        Assert.That(result, Is.True);
        Assert.That(recurringTransaction, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_ShouldReturnFalse_WhenRecurringTransactionBelongsToAnotherUser()
    {
        bool result = await service.DeleteAsync(303, UserId);

        bool stillExists = await context.RecurringTransactions.AnyAsync(rt => rt.Id == 303);

        Assert.That(result, Is.False);
        Assert.That(stillExists, Is.True);
    }

    private async Task SeedDataAsync()
    {
        Category salaryCategory = new Category
        {
            Id = 1,
            Name = "Salary",
            Type = TransactionType.Income,
            IsDefault = true,
            CreatedOn = new DateTime(2026, 1, 1)
        };

        Category foodCategory = new Category
        {
            Id = 2,
            Name = "Food",
            Type = TransactionType.Expense,
            IsDefault = true,
            CreatedOn = new DateTime(2026, 1, 1)
        };

        Category gymCategory = new Category
        {
            Id = 3,
            Name = "Gym",
            Type = TransactionType.Expense,
            IsDefault = false,
            UserId = UserId,
            CreatedOn = new DateTime(2026, 1, 1)
        };

        Category otherUserCategory = new Category
        {
            Id = 4,
            Name = "Other User Category",
            Type = TransactionType.Expense,
            IsDefault = false,
            UserId = OtherUserId,
            CreatedOn = new DateTime(2026, 1, 1)
        };

        await context.Categories.AddRangeAsync(salaryCategory,foodCategory,gymCategory,otherUserCategory);

        await context.RecurringTransactions.AddRangeAsync(
            new RecurringTransaction
            {
                Id = 301,
                Description = "Weekly Groceries",
                Amount = 40m,
                Type = TransactionType.Expense,
                StartDate = new DateTime(2026, 10, 1),
                NextExecutionDate = new DateTime(2026, 10, 8),
                EndDate = null,
                Interval = RecurringInterval.Weekly,
                IsActive = true,
                CategoryId = 2,
                UserId = UserId,
                CreatedOn = new DateTime(2026, 10, 1)
            },
            new RecurringTransaction
            {
                Id = 302,
                Description = "Subscription",
                Amount = 15m,
                Type = TransactionType.Expense,
                StartDate = new DateTime(2026, 10, 5),
                NextExecutionDate = new DateTime(2026, 11, 5),
                EndDate = null,
                Interval = RecurringInterval.Monthly,
                IsActive = true,
                CategoryId = 2,
                UserId = UserId,
                CreatedOn = new DateTime(2026, 10, 5)
            },
            new RecurringTransaction
            {
                Id = 303,
                Description = "Other User Recurring",
                Amount = 100m,
                Type = TransactionType.Expense,
                StartDate = new DateTime(2026, 10, 1),
                NextExecutionDate = new DateTime(2026, 11, 1),
                EndDate = null,
                Interval = RecurringInterval.Monthly,
                IsActive = true,
                CategoryId = 4,
                UserId = OtherUserId,
                CreatedOn = new DateTime(2026, 10, 1)
            });

        await context.SaveChangesAsync();
    }

    private static RecurringTransactionUpdateDto CreateValidUpdateDto()
    {
        return new RecurringTransactionUpdateDto
        {
            Description = "Updated Recurring",
            Amount = 50m,
            Type = TransactionType.Expense,
            StartDate = new DateTime(2026, 11, 1),
            EndDate = null,
            Interval = RecurringInterval.Monthly,
            IsActive = true,
            CategoryId = 2
        };
    }
}