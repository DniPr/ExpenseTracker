namespace ExpenseTracker.Services.Interfaces;

public interface IRecurringTransactionProcessorService
{
    Task ProcessDueRecurringTransactionsAsync(
        CancellationToken cancellationToken = default);
}