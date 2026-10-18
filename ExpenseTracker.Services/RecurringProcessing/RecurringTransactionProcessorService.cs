using ExpenseTracker.Data;
using ExpenseTracker.Data.Models.Entities;
using ExpenseTracker.Data.Models.Enums;
using ExpenseTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Services.RecurringProcessing;

public class RecurringTransactionProcessorService : IRecurringTransactionProcessorService
{
    private readonly ApplicationDbContext dbContext;

    public RecurringTransactionProcessorService(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task ProcessDueRecurringTransactionsAsync(CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;

        List<RecurringTransaction> recurringTransactions =
            await dbContext
                .RecurringTransactions
                .Where(rt =>
                    rt.IsActive &&
                    rt.NextExecutionDate <= now)
                .ToListAsync(cancellationToken);

        foreach (RecurringTransaction recurring in recurringTransactions)
        {
            await ProcessRecurringTransactionAsync(recurring,now,cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessRecurringTransactionAsync(RecurringTransaction recurring,DateTime now,CancellationToken cancellationToken)
    {
        while (recurring.IsActive && recurring.NextExecutionDate <= now)
        {
            if (recurring.EndDate.HasValue && recurring.NextExecutionDate > recurring.EndDate.Value)
            {
                recurring.IsActive = false;
                break;
            }

            bool transactionAlreadyExists =
                await dbContext
                    .Transactions
                    .AnyAsync(
                        t =>
                            t.UserId == recurring.UserId &&
                            t.CategoryId == recurring.CategoryId &&
                            t.Description == recurring.Description &&
                            t.Amount == recurring.Amount &&
                            t.Type == recurring.Type &&
                            t.Date ==
                            recurring.NextExecutionDate,
                        cancellationToken);

            if (!transactionAlreadyExists)
            {
                Transaction transaction =
                    new Transaction
                    {
                        Description =
                            recurring.Description,

                        Amount =
                            recurring.Amount,

                        Date =
                            recurring.NextExecutionDate,

                        Type =
                            recurring.Type,

                        Notes =
                            "Generated automatically from recurring transaction.",

                        CategoryId =
                            recurring.CategoryId,

                        UserId =
                            recurring.UserId,

                        CreatedOn =
                            DateTime.UtcNow
                    };

                await dbContext
                    .Transactions
                    .AddAsync(transaction,cancellationToken);
            }

            DateTime nextExecutionDate =
                CalculateNextExecutionDate(recurring.NextExecutionDate,recurring.Interval);

            if (recurring.EndDate.HasValue && nextExecutionDate > recurring.EndDate.Value)
            {
                recurring.IsActive = false;
            }

            recurring.NextExecutionDate =
                nextExecutionDate;
        }
    }

    private static DateTime CalculateNextExecutionDate(DateTime currentDate,RecurringInterval interval)
    {
        return interval switch
        {
            RecurringInterval.Weekly =>
                currentDate.AddDays(7),

            RecurringInterval.Monthly =>
                currentDate.AddMonths(1),

            RecurringInterval.Yearly =>
                currentDate.AddYears(1),

            _ => throw new ArgumentOutOfRangeException(nameof(interval),interval,"Unsupported recurring interval.")
        };
    }
}