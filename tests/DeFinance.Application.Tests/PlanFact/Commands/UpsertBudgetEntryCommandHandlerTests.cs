using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.PlanFact.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace DeFinance.Application.Tests.PlanFact.Commands;

public class UpsertBudgetEntryCommandHandlerTests
{
    private readonly IBudgetEntryRepository _repository = Substitute.For<IBudgetEntryRepository>();
    private readonly UpsertBudgetEntryCommandHandler _handler;

    public UpsertBudgetEntryCommandHandlerTests()
    {
        _handler = new UpsertBudgetEntryCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenEntryDoesNotExist_ShouldCreateNewEntry()
    {
        var categoryId = Guid.NewGuid();
        _repository.GetAsync(categoryId, 2025, 6, Arg.Any<CancellationToken>()).Returns((BudgetEntry?)null);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new UpsertBudgetEntryCommand(categoryId, 2025, 6, 500m, [
            new BudgetEntryLineRequest("Groceries", 200m),
            new BudgetEntryLineRequest("Transport", 300m),
        ]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        await _repository.Received(1).AddAsync(Arg.Any<BudgetEntry>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEntryExists_ShouldUpdateExistingEntry()
    {
        var categoryId = Guid.NewGuid();
        var existing = BudgetEntry.Create(categoryId, 2025, 6, 300m);
        _repository.GetAsync(categoryId, 2025, 6, Arg.Any<CancellationToken>()).Returns(existing);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new UpsertBudgetEntryCommand(categoryId, 2025, 6, 600m, [
            new BudgetEntryLineRequest("Food", 600m),
        ]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
        existing.PlannedAmount.Should().Be(600m);
        await _repository.DidNotReceive().AddAsync(Arg.Any<BudgetEntry>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyLines_ShouldCreateEntryWithNoLines()
    {
        var categoryId = Guid.NewGuid();
        _repository.GetAsync(categoryId, 2025, 1, Arg.Any<CancellationToken>()).Returns((BudgetEntry?)null);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var command = new UpsertBudgetEntryCommand(categoryId, 2025, 1, 1000m, []);

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<BudgetEntry>(e => e.Lines.Count == 0),
            Arg.Any<CancellationToken>());
    }
}
