using DeFinance.Application.Abstractions;
using StackExchange.Redis;

namespace DeFinance.Infrastructure.Services;

public class RedisEventPublisher(IConnectionMultiplexer redis) : IEventPublisher
{
    public async Task PublishAsync(string channel, string message, CancellationToken ct = default)
    {
        var sub = redis.GetSubscriber();
        await sub.PublishAsync(RedisChannel.Literal(channel), message);
    }
}
