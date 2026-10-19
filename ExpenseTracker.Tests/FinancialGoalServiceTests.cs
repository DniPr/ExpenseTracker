using ExpenseTracker.Data;
using ExpenseTracker.Data.Models.Entities;
using ExpenseTracker.Data.Models.Enums;
using ExpenseTracker.DTOs.FinancialGoals;
using ExpenseTracker.Services.FinancialGoals;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Tests;

[TestFixture]
public class FinancialGoalServiceTests
{
    private ApplicationDbContext context = null!;
    private FinancialGoalService service = null!;

    private const string UserId = "user-1";
    private const string OtherUserId = "user-2";

    [SetUp]
    public async Task SetUp()
    {
        DbContextOptions<ApplicationDbContext> options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"FinancialGoalServiceTests_{Guid.NewGuid()}")
                .Options;

        context = new ApplicationDbContext(options);
        service = new FinancialGoalService(context);

        await SeedDataAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnOnlyGoalsForCurrentUser()
    {
        IEnumerable<FinancialGoalDto> result = await service.GetAllAsync(UserId);

        List<FinancialGoalDto> goals = result.ToList();

        Assert.That(goals, Has.Count.EqualTo(2));
        Assert.That(goals.All(g => g.Id != 403), Is.True);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnGoal_WhenOwnedByUser()
    {
        FinancialGoalDto? result = await service.GetByIdAsync(401, UserId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Emergency Fund"));
        Assert.That(result.TargetAmount, Is.EqualTo(5000m));
        Assert.That(result.Status, Is.EqualTo(GoalStatus.Active));
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnNull_WhenGoalBelongsToAnotherUser()
    {
        FinancialGoalDto? result = await service.GetByIdAsync(403, UserId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_ShouldCreateActiveGoal_WhenCurrentAmountIsBelowTarget()
    {
        FinancialGoalCreateDto dto = new FinancialGoalCreateDto
        {
            Name = "New Laptop",
            Description = "Save for a laptop",
            TargetAmount = 2000m,
            CurrentAmount = 500m,
            TargetDate = DateTime.UtcNow.Date.AddMonths(6)
        };

        FinancialGoalDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Status, Is.EqualTo(GoalStatus.Active));
        Assert.That(result.CurrentAmount, Is.EqualTo(500m));

        FinancialGoal? goal = await context.FinancialGoals.FirstOrDefaultAsync(g => g.Id == result.Id);

        Assert.That(goal, Is.Not.Null);
        Assert.That(goal!.UserId, Is.EqualTo(UserId));
    }

    [Test]
    public async Task CreateAsync_ShouldCreateCompletedGoal_WhenCurrentAmountReachesTarget()
    {
        FinancialGoalCreateDto dto = new FinancialGoalCreateDto
        {
            Name = "Phone",
            TargetAmount = 1000m,
            CurrentAmount = 1000m,
            TargetDate = DateTime.UtcNow.Date.AddMonths(2)
        };

        FinancialGoalDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Status, Is.EqualTo(GoalStatus.Completed));
    }

    [Test]
    public async Task CreateAsync_ShouldReturnNull_WhenTargetAmountIsInvalid()
    {
        FinancialGoalCreateDto dto = new FinancialGoalCreateDto
        {
            Name = "Invalid Goal",
            TargetAmount = 0m,
            CurrentAmount = 0m,
            TargetDate = DateTime.UtcNow.Date.AddMonths(2)
        };

        FinancialGoalDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_ShouldReturnNull_WhenTargetDateIsInThePast()
    {
        FinancialGoalCreateDto dto = new FinancialGoalCreateDto
        {
            Name = "Past Goal",
            TargetAmount = 1000m,
            CurrentAmount = 100m,
            TargetDate = DateTime.UtcNow.Date.AddDays(-1)
        };

        FinancialGoalDto? result = await service.CreateAsync(dto, UserId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateGoal_WhenDataIsValid()
    {
        FinancialGoalUpdateDto dto = new FinancialGoalUpdateDto
        {
            Name = "Updated Emergency Fund",
            Description = "Updated description",
            TargetAmount = 6000m,
            CurrentAmount = 2500m,
            TargetDate = DateTime.UtcNow.Date.AddMonths(8),
            Status = GoalStatus.Active
        };

        bool result = await service.UpdateAsync(401, dto, UserId);

        FinancialGoal goal = await context.FinancialGoals.FirstAsync(g => g.Id == 401);

        Assert.That(result, Is.True);
        Assert.That(goal.Name, Is.EqualTo("Updated Emergency Fund"));
        Assert.That(goal.TargetAmount, Is.EqualTo(6000m));
        Assert.That(goal.CurrentAmount, Is.EqualTo(2500m));
        Assert.That(goal.Status, Is.EqualTo(GoalStatus.Active));
        Assert.That(goal.UpdatedOn, Is.Not.Null);
    }

    [Test]
    public async Task UpdateAsync_ShouldAutomaticallyCompleteGoal_WhenTargetIsReached()
    {
        FinancialGoalUpdateDto dto = new FinancialGoalUpdateDto
        {
            Name = "Emergency Fund",
            TargetAmount = 5000m,
            CurrentAmount = 5000m,
            TargetDate = DateTime.UtcNow.Date.AddMonths(4),
            Status = GoalStatus.Active
        };

        bool result = await service.UpdateAsync(401, dto, UserId);

        FinancialGoal goal = await context.FinancialGoals.FirstAsync(g => g.Id == 401);

        Assert.That(result, Is.True);
        Assert.That(goal.Status, Is.EqualTo(GoalStatus.Completed));
    }

    [Test]
    public async Task UpdateAsync_ShouldChangeCompletedStatusToActive_WhenTargetIsNotReached()
    {
        FinancialGoalUpdateDto dto = new FinancialGoalUpdateDto
        {
            Name = "Vacation",
            TargetAmount = 3000m,
            CurrentAmount = 1500m,
            TargetDate = DateTime.UtcNow.Date.AddMonths(5),
            Status = GoalStatus.Completed
        };

        bool result = await service.UpdateAsync(402, dto, UserId);

        FinancialGoal goal = await context.FinancialGoals.FirstAsync(g => g.Id == 402);

        Assert.That(result, Is.True);
        Assert.That(goal.Status, Is.EqualTo(GoalStatus.Active));
    }

    [Test]
    public async Task UpdateAsync_ShouldReturnFalse_WhenGoalBelongsToAnotherUser()
    {
        FinancialGoalUpdateDto dto = new FinancialGoalUpdateDto
        {
            Name = "Other Goal",
            TargetAmount = 2000m,
            CurrentAmount = 500m,
            TargetDate = DateTime.UtcNow.Date.AddMonths(3),
            Status = GoalStatus.Active
        };

        bool result = await service.UpdateAsync(403, dto, UserId);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task DeleteAsync_ShouldDeleteGoal_WhenOwnedByUser()
    {
        bool result = await service.DeleteAsync(402, UserId);

        FinancialGoal? goal = await context.FinancialGoals.FirstOrDefaultAsync(g => g.Id == 402);

        Assert.That(result, Is.True);
        Assert.That(goal, Is.Null);
    }

    private async Task SeedDataAsync()
    {
        await context.FinancialGoals.AddRangeAsync(
            new FinancialGoal
            {
                Id = 401,
                Name = "Emergency Fund",
                Description = "Build emergency savings",
                TargetAmount = 5000m,
                CurrentAmount = 1500m,
                TargetDate = DateTime.UtcNow.Date.AddMonths(6),
                Status = GoalStatus.Active,
                UserId = UserId,
                CreatedOn = new DateTime(2026, 10, 1)
            },
            new FinancialGoal
            {
                Id = 402,
                Name = "Vacation",
                Description = "Summer vacation",
                TargetAmount = 3000m,
                CurrentAmount = 3000m,
                TargetDate = DateTime.UtcNow.Date.AddMonths(4),
                Status = GoalStatus.Completed,
                UserId = UserId,
                CreatedOn = new DateTime(2026, 10, 2)
            },
            new FinancialGoal
            {
                Id = 403,
                Name = "Other User Goal",
                TargetAmount = 10000m,
                CurrentAmount = 1000m,
                TargetDate = DateTime.UtcNow.Date.AddMonths(10),
                Status = GoalStatus.Active,
                UserId = OtherUserId,
                CreatedOn = new DateTime(2026, 10, 3)
            });

        await context.SaveChangesAsync();
    }
}