using ExpenseTracker.Data;
using ExpenseTracker.Data.Models.Entities;
using ExpenseTracker.Data.Models.Enums;
using ExpenseTracker.DTOs.Budgets;
using ExpenseTracker.Services.Budgets;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Tests;

[TestFixture]
public class BudgetServiceTests
{
    private ApplicationDbContext context = null!;
    private BudgetService service = null!;
    private DateTime now;

    private const string UserId = "user-1";
    private const string OtherUserId = "user-2";

    [SetUp]
    public async Task SetUp()
    {
        now = DateTime.UtcNow;

        DbContextOptions<ApplicationDbContext> options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"BudgetServiceTests_{Guid.NewGuid()}")
                .Options;

        context = new ApplicationDbContext(options);
        service = new BudgetService(context);

        await SeedDataAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnOnlyBudgetsForCurrentUser()
    {
        IEnumerable<BudgetDto> result = await service.GetAllAsync(UserId);

        List<BudgetDto> budgets = result.ToList();

        Assert.That(budgets, Has.Count.EqualTo(2));
        Assert.That(budgets.All(b => b.Id != 203), Is.True);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnBudget_WhenOwnedByUser()
    {
        BudgetDto? result = await service.GetByIdAsync(201, UserId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Amount, Is.EqualTo(500m));
        Assert.That(result.CategoryName, Is.EqualTo("Food"));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnNull_WhenBudgetBelongsToAnotherUser()
    {
        BudgetDto? result = await service.GetByIdAsync(203, UserId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_ShouldCreateMonthlyBudget_WhenDataIsValid()
    {
        DateTime nextMonth = now.AddMonths(1);

        BudgetCreateDto dto = new BudgetCreateDto
        {
            Amount = 150m,
            Period = BudgetPeriod.Monthly,
            Month = nextMonth.Month,
            Year = nextMonth.Year,
            CategoryId = 3
        };

        BudgetDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Amount, Is.EqualTo(150m));
        Assert.That(result.CategoryName, Is.EqualTo("Gym"));

        Budget? budget = await context.Budgets.FirstOrDefaultAsync(b => b.Id == result.Id);

        Assert.That(budget, Is.Not.Null);
        Assert.That(budget!.UserId, Is.EqualTo(UserId));
    }

    [Test]
    public async Task CreateAsync_ShouldCreateYearlyOverallBudget_WhenDataIsValid()
    {
        BudgetCreateDto dto = new BudgetCreateDto
        {
            Amount = 6000m,
            Period = BudgetPeriod.Yearly,
            Month = null,
            Year = now.Year + 1,
            CategoryId = null
        };

        BudgetDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Amount, Is.EqualTo(6000m));
        Assert.That(result.CategoryId, Is.Null);
        Assert.That(result.CategoryName, Is.Null);
    }

    [Test]
    public async Task CreateAsync_ShouldReturnNull_WhenPeriodAndMonthAreInvalid()
    {
        BudgetCreateDto dto = new BudgetCreateDto
        {
            Amount = 1000m,
            Period = BudgetPeriod.Yearly,
            Month = 5,
            Year = now.Year + 1,
            CategoryId = null
        };

        BudgetDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_ShouldReturnNull_WhenBudgetPeriodIsInThePast()
    {
        BudgetCreateDto dto = new BudgetCreateDto
        {
            Amount = 1000m,
            Period = BudgetPeriod.Yearly,
            Month = null,
            Year = now.Year - 1,
            CategoryId = null
        };

        BudgetDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_ShouldReturnNull_WhenCategoryIsIncomeCategory()
    {
        DateTime nextMonth = now.AddMonths(1);

        BudgetCreateDto dto = new BudgetCreateDto
        {
            Amount = 500m,
            Period = BudgetPeriod.Monthly,
            Month = nextMonth.Month,
            Year = nextMonth.Year,
            CategoryId = 1
        };

        BudgetDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_ShouldReturnNull_WhenDuplicateBudgetExists()
    {
        BudgetCreateDto dto = new BudgetCreateDto
        {
            Amount = 700m,
            Period = BudgetPeriod.Monthly,
            Month = now.Month,
            Year = now.Year,
            CategoryId = 2
        };

        BudgetDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateBudget_WhenDataIsValid()
    {
        DateTime nextMonth = now.AddMonths(1);

        BudgetUpdateDto dto = new BudgetUpdateDto
        {
            Amount = 750m,
            Period = BudgetPeriod.Monthly,
            Month = nextMonth.Month,
            Year = nextMonth.Year,
            CategoryId = 2
        };

        bool result = await service.UpdateAsync(201, dto, UserId);

        Budget budget = await context.Budgets.FirstAsync(b => b.Id == 201);

        Assert.That(result, Is.True);
        Assert.That(budget.Amount, Is.EqualTo(750m));
        Assert.That(budget.Month, Is.EqualTo(nextMonth.Month));
        Assert.That(budget.Year, Is.EqualTo(nextMonth.Year));
        Assert.That(budget.UpdatedOn, Is.Not.Null);
    }

    [Test]
    public async Task UpdateAsync_ShouldReturnFalse_WhenBudgetBelongsToAnotherUser()
    {
        BudgetUpdateDto dto = new BudgetUpdateDto
        {
            Amount = 900m,
            Period = BudgetPeriod.Yearly,
            Month = null,
            Year = now.Year + 1,
            CategoryId = null
        };

        bool result = await service.UpdateAsync(203, dto, UserId);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteAsync_ShouldDeleteBudget_WhenOwnedByUser()
    {
        bool result = await service.DeleteAsync(202, UserId);

        Budget? budget = await context.Budgets.FirstOrDefaultAsync(b => b.Id == 202);

        Assert.That(result, Is.True);
        Assert.That(budget, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_ShouldReturnFalse_WhenBudgetBelongsToAnotherUser()
    {
        bool result = await service.DeleteAsync(203, UserId);

        bool stillExists = await context.Budgets.AnyAsync(b => b.Id == 203);

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

        await context.Categories.AddRangeAsync(salaryCategory,foodCategory,gymCategory);

        await context.Budgets.AddRangeAsync(
            new Budget
            {
                Id = 201,
                Amount = 500m,
                Period = BudgetPeriod.Monthly,
                Month = now.Month,
                Year = now.Year,
                CategoryId = 2,
                UserId = UserId,
                CreatedOn = new DateTime(2026, 10, 1)
            },
            new Budget
            {
                Id = 202,
                Amount = 5000m,
                Period = BudgetPeriod.Yearly,
                Month = null,
                Year = now.Year,
                CategoryId = null,
                UserId = UserId,
                CreatedOn = new DateTime(2026, 10, 1)
            },
            new Budget
            {
                Id = 203,
                Amount = 300m,
                Period = BudgetPeriod.Monthly,
                Month = now.Month,
                Year = now.Year,
                CategoryId = 2,
                UserId = OtherUserId,
                CreatedOn = new DateTime(2026, 10, 1)
            });

        await context.SaveChangesAsync();
    }
}