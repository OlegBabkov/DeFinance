using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Counterparties.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Counterparties.Commands;

public class UpdateCounterpartyCommandHandlerTests
{
    private readonly ICounterpartyRepository _repository = Substitute.For<ICounterpartyRepository>();
    private readonly UpdateCounterpartyCommandHandler _handler;

    public UpdateCounterpartyCommandHandlerTests()
    {
        _handler = new UpdateCounterpartyCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenExists_ShouldUpdateAndReturnResponse()
    {
        var id = Guid.NewGuid();
        var counterparty = Counterparty.Create("Amazon", CounterpartyType.Company, null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(counterparty);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(
            new UpdateCounterpartyCommand(id, "Amazon DE", CounterpartyType.Company, "contact@amazon.de"),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Amazon DE");
        result.ContactInfo.Should().Be("contact@amazon.de");
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Counterparty?)null);

        var result = await _handler.Handle(
            new UpdateCounterpartyCommand(Guid.NewGuid(), "Name", CounterpartyType.Other, null),
            CancellationToken.None);

        result.Should().BeNull();
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
