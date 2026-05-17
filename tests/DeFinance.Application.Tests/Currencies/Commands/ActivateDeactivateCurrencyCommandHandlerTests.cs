using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Currencies.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Currencies.Commands;

public class ActivateDeactivateCurrencyCommandHandlerTests
{
    private readonly ICurrencyRepository _repository = Substitute.For<ICurrencyRepository>();

    [Fact]
    public async Task Activate_WhenCurrencyExists_ShouldSetIsActiveTrue()
    {
        var id = Guid.NewGuid();
        var currency = Currency.Create("USD", "US Dollar", "$");
        currency.Deactivate();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(currency);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await new ActivateCurrencyCommandHandler(_repository)
            .Handle(new ActivateCurrencyCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WhenCurrencyNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Currency?)null);

        var result = await new ActivateCurrencyCommandHandler(_repository)
            .Handle(new ActivateCurrencyCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Deactivate_WhenCurrencyExists_ShouldSetIsActiveFalse()
    {
        var id = Guid.NewGuid();
        var currency = Currency.Create("EUR", "Euro", "€");
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(currency);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await new DeactivateCurrencyCommandHandler(_repository)
            .Handle(new DeactivateCurrencyCommand(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Deactivate_WhenCurrencyNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Currency?)null);

        var result = await new DeactivateCurrencyCommandHandler(_repository)
            .Handle(new DeactivateCurrencyCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
