namespace DeFinance.Application.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync(string channel, string message, CancellationToken ct = default);
}
