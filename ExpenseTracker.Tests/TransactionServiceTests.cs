using ExpenseTracker.Data;
using ExpenseTracker.Data.Models.Entities;
using ExpenseTracker.Data.Models.Enums;
using ExpenseTracker.DTOs.Transactions;
using ExpenseTracker.Services.Transactions;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Tests;

[TestFixture]
public class TransactionServiceTests
{
    private ApplicationDbContext context = null!;
    private TransactionService service = null!;

    private const string UserId = "user-1";
    private const string OtherUserId = "user-2";

    [SetUp]
    public async Task SetUp()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"ExpenseTrackerTests_{Guid.NewGuid()}")
                .Options;

        context =
            new ApplicationDbContext(options);

        service =
            new TransactionService(context);

        await SeedDataAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Test]
    public async Task GetAllAsync_ShouldReturnOnlyTransactionsForCurrentUser()
    {
        TransactionFilterDto filter = new TransactionFilterDto();

        IEnumerable<TransactionDto> result =
            await service.GetAllAsync(filter,UserId);

        List<TransactionDto> transactions =
            result.ToList();

        Assert.That(transactions,Has.Count.EqualTo(3));

        Assert.That(transactions.All(t =>t.Id != 104),Is.True);
    }

    [Test]
    public async Task GetAllAsync_ShouldApplyFilters()
    {
        TransactionFilterDto filter =
            new TransactionFilterDto
            {
                FromDate =
                    new DateTime(2026, 10, 14),

                ToDate =
                    new DateTime(2026, 10, 17),

                Type =
                    TransactionType.Expense,

                CategoryId = 2,

                SearchTerm = "lunch"
            };

        IEnumerable<TransactionDto> result =
            await service.GetAllAsync(filter,UserId);

        List<TransactionDto> transactions =
            result.ToList();

        Assert.That(transactions,Has.Count.EqualTo(1));

        Assert.Multiple(() =>
        {
            Assert.That(transactions[0].Description,Is.EqualTo("Lunch"));

            Assert.That(transactions[0].CategoryName,Is.EqualTo("Food"));
        });
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnTransaction_WhenOwnedByUser()
    {
        TransactionDto? result =
            await service.GetByIdAsync(102,UserId);

        Assert.That(result,Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(result!.Description,Is.EqualTo("Gym"));

            Assert.That(result.Amount,Is.EqualTo(50m));

            Assert.That(result.CategoryName,Is.EqualTo("Gym"));
        });
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnNull_WhenTransactionBelongsToAnotherUser()
    {
        TransactionDto? result =
            await service.GetByIdAsync(
                104,
                UserId);

        Assert.That(
            result,
            Is.Null);
    }

    [Test]
    public async Task CreateAsync_ShouldCreateTransaction_WhenCategoryIsValid()
    {
        TransactionCreateDto dto =
            new TransactionCreateDto
            {
                Description = "Dinner",
                Amount = 35m,
                Date =
                    new DateTime(2026, 10, 18),

                Type =
                    TransactionType.Expense,

                Notes = "Restaurant",
                CategoryId = 2
            };

        TransactionDto? result =
            await service.CreateAsync(
                dto,
                UserId);

        Transaction? transaction =
            await context.Transactions
                .FirstOrDefaultAsync(t =>
                    t.Description == "Dinner");

        Assert.That(
            result,
            Is.Not.Null);

        Assert.That(
            transaction,
            Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(
                result!.Amount,
                Is.EqualTo(35m));

            Assert.That(
                result.CategoryName,
                Is.EqualTo("Food"));

            Assert.That(
                transaction!.UserId,
                Is.EqualTo(UserId));
        });
    }

    [Test]
    public async Task CreateAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        TransactionCreateDto dto =
            new TransactionCreateDto
            {
                Description = "Invalid",
                Amount = 20m,
                Date =
                    new DateTime(2026, 10, 18),

                Type =
                    TransactionType.Expense,

                CategoryId = 999
            };

        TransactionDto? result =
            await service.CreateAsync(
                dto,
                UserId);

        Assert.That(
            result,
            Is.Null);
    }

    [Test]
    public async Task CreateAsync_ShouldReturnNull_WhenCategoryTypeDoesNotMatch()
    {
        TransactionCreateDto dto =
            new TransactionCreateDto
            {
                Description = "Wrong Type",
                Amount = 100m,
                Date =
                    new DateTime(2026, 10, 18),

                Type =
                    TransactionType.Income,

                CategoryId = 2
            };

        TransactionDto? result =
            await service.CreateAsync(
                dto,
                UserId);

        Assert.That(
            result,
            Is.Null);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateTransaction_WhenDataIsValid()
    {
        TransactionUpdateDto dto =
            new TransactionUpdateDto
            {
                Description = "Updated Gym",
                Amount = 75m,
                Date =
                    new DateTime(2026, 10, 19),

                Type =
                    TransactionType.Expense,

                Notes = "Updated note",
                CategoryId = 2
            };

        bool result =
            await service.UpdateAsync(
                102,
                dto,
                UserId);

        Transaction transaction =
            await context.Transactions
                .FirstAsync(t =>
                    t.Id == 102);

        Assert.That(
            result,
            Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(
                transaction.Description,
                Is.EqualTo("Updated Gym"));

            Assert.That(
                transaction.Amount,
                Is.EqualTo(75m));

            Assert.That(
                transaction.CategoryId,
                Is.EqualTo(2));

            Assert.That(
                transaction.UpdatedOn,
                Is.Not.Null);
        });
    }

    [Test]
    public async Task UpdateAsync_ShouldReturnFalse_WhenTransactionBelongsToAnotherUser()
    {
        TransactionUpdateDto dto =
            CreateValidUpdateDto();

        bool result =
            await service.UpdateAsync(
                104,
                dto,
                UserId);

        Assert.That(
            result,
            Is.False);
    }

    [Test]
    public async Task DeleteAsync_ShouldDeleteTransaction_WhenOwnedByUser()
    {
        bool result =
            await service.DeleteAsync(102,UserId);

        Transaction? transaction =
            await context.Transactions
                .FirstOrDefaultAsync(t =>
                    t.Id == 102);

        Assert.That(result,Is.True);

        Assert.That(transaction,Is.Null);
    }

    [Test]
    public async Task DeleteAsync_ShouldReturnFalse_WhenTransactionBelongsToAnotherUser()
    {
        bool result =
            await service.DeleteAsync(104,UserId);

        bool stillExists =
            await context.Transactions
                .AnyAsync(t =>
                    t.Id == 104);

        Assert.That(result,Is.False);

        Assert.That(stillExists,Is.True);
    }

    private async Task SeedDataAsync()
    {
        Category salaryCategory =
            new Category
            {
                Id = 1,
                Name = "Salary",
                Type =
                    TransactionType.Income,
                IsDefault = true,
                CreatedOn =
                    new DateTime(2026,1,1)
            };

        Category foodCategory =
            new Category
            {
                Id = 2,
                Name = "Food",
                Type =
                    TransactionType.Expense,
                IsDefault = true,
                CreatedOn =
                    new DateTime(2026,1,1)
            };

        Category gymCategory =
            new Category
            {
                Id = 3,
                Name = "Gym",
                Type =
                    TransactionType.Expense,
                IsDefault = false,
                UserId = UserId,
                CreatedOn =
                    new DateTime(2026,1,1)
            };

        Category otherUserCategory =
            new Category
            {
                Id = 4,
                Name = "Other User Category",
                Type =
                    TransactionType.Expense,
                IsDefault = false,
                UserId = OtherUserId,
                CreatedOn =
                    new DateTime(2026,1,1)
            };

        await context.Categories.AddRangeAsync(salaryCategory,foodCategory,gymCategory,otherUserCategory);

        await context.Transactions.AddRangeAsync(
            new Transaction
            {
                Id = 101,
                Description = "Salary",
                Amount = 1500m,
                Date =
                    new DateTime(2026,10,10),

                Type =
                    TransactionType.Income,

                CategoryId = 1,
                UserId = UserId,
                CreatedOn =
                    new DateTime(2026,10,10,10,0,0)
            },

            new Transaction
            {
                Id = 102,
                Description = "Gym",
                Amount = 50m,
                Date =
                    new DateTime(2026,10,15),

                Type =
                    TransactionType.Expense,

                Notes = "Membership",
                CategoryId = 3,
                UserId = UserId,
                CreatedOn =
                    new DateTime(2026,10,15,10,0,0)
            },

            new Transaction
            {
                Id = 103,
                Description = "Lunch",
                Amount = 15m,
                Date =
                    new DateTime(2026,10,16),

                Type =
                    TransactionType.Expense,

                Notes = "Business lunch",
                CategoryId = 2,
                UserId = UserId,
                CreatedOn =
                    new DateTime(2026,10,16,10,0,0)
            },

            new Transaction
            {
                Id = 104,
                Description = "Other User Expense",
                Amount = 200m,
                Date =
                    new DateTime(2026,10,17),

                Type =
                    TransactionType.Expense,

                CategoryId = 4,
                UserId = OtherUserId,
                CreatedOn =
                    new DateTime(2026,10,17,10,0,0)
            });

        await context.SaveChangesAsync();
    }

    private static TransactionUpdateDto CreateValidUpdateDto()
    {
        return new TransactionUpdateDto
        {
            Description = "Updated Transaction",
            Amount = 60m,
            Date =
                new DateTime(2026,10,19),

            Type =
                TransactionType.Expense,

            Notes = "Updated",
            CategoryId = 2
        };
    }
}