using DeFinance.Application.ExchangeRates.Commands;
using MediatR;

namespace DeFinance.Api.BackgroundServices;

public class ExchangeRateSyncService(
    IServiceScopeFactory scopeFactory,
    ILogger<ExchangeRateSyncService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SyncAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(1); // 01:00 UTC — after ECB daily publish
            await Task.Delay(nextRun - now, stoppingToken);
            await SyncAsync(stoppingToken);
        }
    }

    private async Task SyncAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var count = await sender.Send(new SyncExchangeRatesCommand(), ct);
            logger.LogInformation("Exchange rate sync completed: {Count} rates updated", count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Exchange rate sync failed");
        }
    }
}
