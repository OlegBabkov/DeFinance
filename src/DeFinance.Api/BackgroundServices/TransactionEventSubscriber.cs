using DeFinance.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace DeFinance.Api.BackgroundServices;

public class TransactionEventSubscriber(
    IConnectionMultiplexer redis,
    IHubContext<NotificationsHub> hubContext) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sub = redis.GetSubscriber();
        sub.Subscribe(RedisChannel.Literal("transactions:changed"), async (_, message) =>
            await hubContext.Clients.All.SendAsync("TransactionChanged", message.ToString(), stoppingToken));
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var sub = redis.GetSubscriber();
        await sub.UnsubscribeAllAsync();
        await base.StopAsync(cancellationToken);
    }
}
