using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Currencies.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Currencies.Commands;

public class UpdateCurrencyCommandHandlerTests
{
    private readonly ICurrencyRepository _repository = Substitute.For<ICurrencyRepository>();
    private readonly UpdateCurrencyCommandHandler _handler;

    public UpdateCurrencyCommandHandlerTests()
    {
        _handler = new UpdateCurrencyCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenCurrencyExists_ShouldUpdateAndReturnResponse()
    {
        var id = Guid.NewGuid();
        var currency = Currency.Create("USD", "US Dollar", "$");
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(currency);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(new UpdateCurrencyCommand(id, "Updated Dollar", "US$"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Dollar");
        result.Symbol.Should().Be("US$");
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCurrencyNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Currency?)null);

        var result = await _handler.Handle(new UpdateCurrencyCommand(Guid.NewGuid(), "Name", "$"), CancellationToken.None);

        result.Should().BeNull();
        await _repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
