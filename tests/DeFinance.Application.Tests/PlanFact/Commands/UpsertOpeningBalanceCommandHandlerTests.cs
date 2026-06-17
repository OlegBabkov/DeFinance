using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.PlanFact.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace DeFinance.Application.Tests.PlanFact.Commands;

public class UpsertOpeningBalanceCommandHandlerTests
{
    private readonly IOpeningBalanceOverrideRepository _repository = Substitute.For<IOpeningBalanceOverrideRepository>();
    private readonly UpsertOpeningBalanceCommandHandler _handler;

    public UpsertOpeningBalanceCommandHandlerTests()
    {
        _handler = new UpsertOpeningBalanceCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenOverrideDoesNotExist_ShouldCreateNewOverride()
    {
        _repository.GetAsync(2025, 6, Arg.Any<CancellationToken>()).Returns((OpeningBalanceOverride?)null);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new UpsertOpeningBalanceCommand(2025, 6, 10000m), CancellationToken.None);

        result.Should().Be(Unit.Value);
        await _repository.Received(1).AddAsync(Arg.Any<OpeningBalanceOverride>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOverrideExists_ShouldUpdateExistingAmount()
    {
        var existing = OpeningBalanceOverride.Create(2025, 6, 5000m);
        _repository.GetAsync(2025, 6, Arg.Any<CancellationToken>()).Returns(existing);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        await _handler.Handle(new UpsertOpeningBalanceCommand(2025, 6, 9999m), CancellationToken.None);

        existing.Amount.Should().Be(9999m);
        await _repository.DidNotReceive().AddAsync(Arg.Any<OpeningBalanceOverride>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
