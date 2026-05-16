using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Currencies.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Currencies.Commands;

public class CreateCurrencyCommandHandlerTests
{
    private readonly ICurrencyRepository _repository = Substitute.For<ICurrencyRepository>();
    private readonly CreateCurrencyCommandHandler _handler;

    public CreateCurrencyCommandHandlerTests()
    {
        _handler = new CreateCurrencyCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldCreateCurrencyAndReturnResponse()
    {
        var command = new CreateCurrencyCommand("usd", "US Dollar", "$");
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Code.Should().Be("USD");
        result.Name.Should().Be("US Dollar");
        result.Symbol.Should().Be("$");
        result.IsActive.Should().BeTrue();

        await _repository.Received(1).AddAsync(Arg.Any<Currency>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUppercaseCode()
    {
        var command = new CreateCurrencyCommand("eur", "Euro", "€");
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("EUR");
    }
}
