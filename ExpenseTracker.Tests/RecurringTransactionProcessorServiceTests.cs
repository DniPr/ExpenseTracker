using ExpenseTracker.Data;
using ExpenseTracker.Data.Models.Entities;
using ExpenseTracker.Data.Models.Enums;
using ExpenseTracker.Services.RecurringProcessing;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Tests;

[TestFixture]
public class RecurringTransactionProcessorServiceTests
{
    private ApplicationDbContext context = null!;
    private RecurringTransactionProcessorService service = null!;

    private const string UserId = "user-1";

    [SetUp]
    public async Task SetUp()
    {
        DbContextOptions<ApplicationDbContext> options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"RecurringProcessorTests_{Guid.NewGuid()}")
                .Options;

        context = new ApplicationDbContext(options);
        service = new RecurringTransactionProcessorService(context);

        await SeedCategoriesAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        await context.Database.EnsureDeletedAsync();
        await context.DisposeAsync();
    }

    [Test]
    public async Task ProcessDueRecurringTransactionsAsync_ShouldCreateTransaction_WhenRecurringTransactionIsDue()
    {
        DateTime dueDate = DateTime.UtcNow.Date.AddDays(-1);

        RecurringTransaction recurring = CreateRecurringTransaction(
            id: 501,
            nextExecutionDate: dueDate,
            interval: RecurringInterval.Weekly);

        await context.RecurringTransactions.AddAsync(recurring);
        await context.SaveChangesAsync();

        await service.ProcessDueRecurringTransactionsAsync();

        Transaction? transaction = await context.Transactions.FirstOrDefaultAsync();

        Assert.That(transaction, Is.Not.Null);
        Assert.That(transaction!.Description, Is.EqualTo("Recurring Expense"));
        Assert.That(transaction.Amount, Is.EqualTo(50m));
        Assert.That(transaction.Date, Is.EqualTo(dueDate));
        Assert.That(transaction.UserId, Is.EqualTo(UserId));
        Assert.That(transaction.CategoryId, Is.EqualTo(1));
    }

    [Test]
    public async Task ProcessDueRecurringTransactionsAsync_ShouldAdvanceNextExecutionDate()
    {
        DateTime dueDate = DateTime.UtcNow.Date.AddDays(-1);

        RecurringTransaction recurring = CreateRecurringTransaction(
            id: 502,
            nextExecutionDate: dueDate,
            interval: RecurringInterval.Weekly);

        await context.RecurringTransactions.AddAsync(recurring);
        await context.SaveChangesAsync();

        await service.ProcessDueRecurringTransactionsAsync();

        RecurringTransaction updated = await context.RecurringTransactions.FirstAsync(rt => rt.Id == 502);

        Assert.That(updated.NextExecutionDate, Is.EqualTo(dueDate.AddDays(7)));
    }

    [Test]
    public async Task ProcessDueRecurringTransactionsAsync_ShouldNotProcessFutureRecurringTransaction()
    {
        DateTime futureDate = DateTime.UtcNow.Date.AddDays(5);

        RecurringTransaction recurring = CreateRecurringTransaction(
            id: 503,
            nextExecutionDate: futureDate,
            interval: RecurringInterval.Weekly);

        await context.RecurringTransactions.AddAsync(recurring);
        await context.SaveChangesAsync();

        await service.ProcessDueRecurringTransactionsAsync();

        int transactionCount = await context.Transactions.CountAsync();

        Assert.That(transactionCount, Is.EqualTo(0));
    }

    [Test]
    public async Task ProcessDueRecurringTransactionsAsync_ShouldNotProcessInactiveRecurringTransaction()
    {
        DateTime dueDate = DateTime.UtcNow.Date.AddDays(-1);

        RecurringTransaction recurring = CreateRecurringTransaction(
            id: 504,
            nextExecutionDate: dueDate,
            interval: RecurringInterval.Weekly);

        recurring.IsActive = false;

        await context.RecurringTransactions.AddAsync(recurring);
        await context.SaveChangesAsync();

        await service.ProcessDueRecurringTransactionsAsync();

        int transactionCount = await context.Transactions.CountAsync();

        Assert.That(transactionCount, Is.EqualTo(0));
    }

    [Test]
    public async Task ProcessDueRecurringTransactionsAsync_ShouldNotCreateDuplicateTransaction()
    {
        DateTime dueDate = DateTime.UtcNow.Date.AddDays(-1);

        RecurringTransaction recurring = CreateRecurringTransaction(
            id: 505,
            nextExecutionDate: dueDate,
            interval: RecurringInterval.Weekly);

        Transaction existingTransaction = new Transaction
        {
            Id = 601,
            Description = recurring.Description,
            Amount = recurring.Amount,
            Date = dueDate,
            Type = recurring.Type,
            Notes = "Existing transaction",
            CategoryId = recurring.CategoryId,
            UserId = recurring.UserId,
            CreatedOn = DateTime.UtcNow.AddDays(-2)
        };

        await context.RecurringTransactions.AddAsync(recurring);
        await context.Transactions.AddAsync(existingTransaction);
        await context.SaveChangesAsync();

        await service.ProcessDueRecurringTransactionsAsync();

        int transactionCount = await context.Transactions.CountAsync();

        Assert.That(transactionCount, Is.EqualTo(1));
    }

    [Test]
    public async Task ProcessDueRecurringTransactionsAsync_ShouldCatchUpMultipleMissedWeeklyExecutions()
    {
        DateTime now = DateTime.UtcNow;
        DateTime firstDueDate = now.Date.AddDays(-22);

        RecurringTransaction recurring = CreateRecurringTransaction(
            id: 506,
            nextExecutionDate: firstDueDate,
            interval: RecurringInterval.Weekly);

        await context.RecurringTransactions.AddAsync(recurring);
        await context.SaveChangesAsync();

        await service.ProcessDueRecurringTransactionsAsync();

        List<Transaction> transactions = await context.Transactions.OrderBy(t => t.Date).ToListAsync();

        Assert.That(transactions, Has.Count.EqualTo(4));
        Assert.That(transactions[0].Date, Is.EqualTo(firstDueDate));
        Assert.That(transactions[1].Date, Is.EqualTo(firstDueDate.AddDays(7)));
        Assert.That(transactions[2].Date, Is.EqualTo(firstDueDate.AddDays(14)));
        Assert.That(transactions[3].Date, Is.EqualTo(firstDueDate.AddDays(21)));
    }

    [Test]
    public async Task ProcessDueRecurringTransactionsAsync_ShouldDeactivateRecurringTransaction_WhenEndDateIsReached()
    {
        DateTime dueDate = DateTime.UtcNow.Date.AddDays(-1);

        RecurringTransaction recurring = CreateRecurringTransaction(
            id: 507,
            nextExecutionDate: dueDate,
            interval: RecurringInterval.Weekly);

        recurring.EndDate = dueDate;

        await context.RecurringTransactions.AddAsync(recurring);
        await context.SaveChangesAsync();

        await service.ProcessDueRecurringTransactionsAsync();

        RecurringTransaction updated = await context.RecurringTransactions.FirstAsync(rt => rt.Id == 507);

        Assert.That(updated.IsActive, Is.False);
        Assert.That(await context.Transactions.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task ProcessDueRecurringTransactionsAsync_ShouldDeactivateWithoutCreatingTransaction_WhenNextExecutionIsAfterEndDate()
    {
        DateTime dueDate = DateTime.UtcNow.Date.AddDays(-1);

        RecurringTransaction recurring = CreateRecurringTransaction(
            id: 508,
            nextExecutionDate: dueDate,
            interval: RecurringInterval.Weekly);

        recurring.EndDate = dueDate.AddDays(-1);

        await context.RecurringTransactions.AddAsync(recurring);
        await context.SaveChangesAsync();

        await service.ProcessDueRecurringTransactionsAsync();

        RecurringTransaction updated = await context.RecurringTransactions.FirstAsync(rt => rt.Id == 508);

        Assert.That(updated.IsActive, Is.False);
        Assert.That(await context.Transactions.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task ProcessDueRecurringTransactionsAsync_ShouldUseCorrectMonthlyInterval()
    {
        DateTime now = DateTime.UtcNow;
        DateTime dueDate = now.Date.AddMonths(-1);

        RecurringTransaction recurring = CreateRecurringTransaction(
            id: 509,
            nextExecutionDate: dueDate,
            interval: RecurringInterval.Monthly);

        await context.RecurringTransactions.AddAsync(recurring);
        await context.SaveChangesAsync();

        await service.ProcessDueRecurringTransactionsAsync();

        RecurringTransaction updated = await context.RecurringTransactions.FirstAsync(rt => rt.Id == 509);

        Assert.That(updated.NextExecutionDate, Is.EqualTo(dueDate.AddMonths(2)));
        Assert.That(await context.Transactions.CountAsync(), Is.EqualTo(2));
    }

    private async Task SeedCategoriesAsync()
    {
        Category category = new Category
        {
            Id = 1,
            Name = "Food",
            Type = TransactionType.Expense,
            IsDefault = true,
            CreatedOn = new DateTime(2026, 1, 1)
        };

        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();
    }

    private static RecurringTransaction CreateRecurringTransaction(
        int id,
        DateTime nextExecutionDate,
        RecurringInterval interval)
    {
        return new RecurringTransaction
        {
            Id = id,
            Description = "Recurring Expense",
            Amount = 50m,
            Type = TransactionType.Expense,
            StartDate = nextExecutionDate.AddMonths(-1),
            NextExecutionDate = nextExecutionDate,
            EndDate = null,
            Interval = interval,
            IsActive = true,
            CategoryId = 1,
            UserId = UserId,
            CreatedOn = DateTime.UtcNow.AddMonths(-2)
        };
    }
}