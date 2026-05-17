using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Counterparties.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Counterparties.Commands;

public class ActivateDeactivateCounterpartyCommandHandlerTests
{
    private readonly ICounterpartyRepository _repository = Substitute.For<ICounterpartyRepository>();

    [Fact]
    public async Task Activate_WhenExists_ShouldSetIsActiveTrue()
    {
        var id = Guid.NewGuid();
        var counterparty = Counterparty.Create("Rewe", CounterpartyType.Company, null);
        counterparty.Deactivate();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(counterparty);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await new ActivateCounterpartyCommandHandler(_repository)
            .Handle(new ActivateCounterpartyCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WhenNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Counterparty?)null);

        var result = await new ActivateCounterpartyCommandHandler(_repository)
            .Handle(new ActivateCounterpartyCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Deactivate_WhenExists_ShouldSetIsActiveFalse()
    {
        var id = Guid.NewGuid();
        var counterparty = Counterparty.Create("Aldi Nord", CounterpartyType.Company, null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(counterparty);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await new DeactivateCounterpartyCommandHandler(_repository)
            .Handle(new DeactivateCounterpartyCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WhenNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Counterparty?)null);

        var result = await new DeactivateCounterpartyCommandHandler(_repository)
            .Handle(new DeactivateCounterpartyCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
