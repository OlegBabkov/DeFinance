using DeFinance.Api.Hubs;
using DeFinance.Contracts.Messages;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace DeFinance.Api.BackgroundServices;

public class ReportGeneratedSubscriber(
    IConnectionMultiplexer redis,
    IHubContext<NotificationsHub> hubContext) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sub = redis.GetSubscriber();
        sub.Subscribe(RedisChannel.Literal("reports:generated"), async (_, message) =>
        {
            var msg = System.Text.Json.JsonSerializer.Deserialize<ReportGeneratedMessage>(message.ToString());
            if (msg is null) return;

            await hubContext.Clients.All.SendAsync("ReportGenerated", new
            {
                reportId = msg.ReportId,
                userId   = msg.UserId,
                success  = msg.Success,
                error    = msg.ErrorMessage
            }, stoppingToken);
        });
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var sub = redis.GetSubscriber();
        await sub.UnsubscribeAllAsync();
        await base.StopAsync(cancellationToken);
    }
}
