using ExpenseTracker.Services.Interfaces;

namespace ExpenseTracker.Api.BackgroundServices;

public class RecurringTransactionBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly ILogger<RecurringTransactionBackgroundService> logger;

    public RecurringTransactionBackgroundService(IServiceScopeFactory serviceScopeFactory,
        ILogger<RecurringTransactionBackgroundService> logger)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Recurring transaction background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope =
                    serviceScopeFactory.CreateScope();

                IRecurringTransactionProcessorService processor =
                    scope.ServiceProvider
                        .GetRequiredService<IRecurringTransactionProcessorService>();

                await processor
                    .ProcessDueRecurringTransactionsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,"An error occurred while processing recurring transactions.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1),stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        logger.LogInformation("Recurring transaction background service stopped.");
    }
}