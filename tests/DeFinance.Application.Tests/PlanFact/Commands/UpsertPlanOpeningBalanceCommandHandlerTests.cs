using DeFinance.Application.Abstractions;
using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.PlanFact.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using MediatR;
using NSubstitute;

namespace DeFinance.Application.Tests.PlanFact.Commands;

public class UpsertPlanOpeningBalanceCommandHandlerTests
{
    private readonly IOpeningBalanceOverrideRepository _repository = Substitute.For<IOpeningBalanceOverrideRepository>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly UpsertPlanOpeningBalanceCommandHandler _handler;

    public UpsertPlanOpeningBalanceCommandHandlerTests()
    {
        _currentUserService.UserId.Returns(Guid.NewGuid());
        _handler = new UpsertPlanOpeningBalanceCommandHandler(_repository, _currentUserService);
    }

    [Fact]
    public async Task Handle_WhenOverrideDoesNotExist_ShouldCreateNewPlanOverride()
    {
        _repository.GetAsync(2025, 7, Arg.Any<CancellationToken>()).Returns((OpeningBalanceOverride?)null);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new UpsertPlanOpeningBalanceCommand(2025, 7, 8000m), CancellationToken.None);

        result.Should().Be(Unit.Value);
        await _repository.Received(1).AddAsync(
            Arg.Is<OpeningBalanceOverride>(o => o.PlanAmount == 8000m && o.Amount == null),
            Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOverrideExists_ShouldUpdatePlanAmount()
    {
        var existing = OpeningBalanceOverride.Create(2025, 7, 5000m, Guid.NewGuid());
        _repository.GetAsync(2025, 7, Arg.Any<CancellationToken>()).Returns(existing);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        await _handler.Handle(new UpsertPlanOpeningBalanceCommand(2025, 7, 12000m), CancellationToken.None);

        existing.PlanAmount.Should().Be(12000m);
        await _repository.DidNotReceive().AddAsync(Arg.Any<OpeningBalanceOverride>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
