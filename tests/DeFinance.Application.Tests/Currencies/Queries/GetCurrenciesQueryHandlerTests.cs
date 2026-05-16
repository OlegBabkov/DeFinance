using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Currencies.Queries;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Currencies.Queries;

public class GetCurrenciesQueryHandlerTests
{
    private readonly ICurrencyRepository _repository = Substitute.For<ICurrencyRepository>();

    [Fact]
    public async Task GetAll_ShouldReturnMappedResponses()
    {
        var currencies = new List<Currency>
        {
            Currency.Create("USD", "US Dollar", "$"),
            Currency.Create("EUR", "Euro", "€")
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(currencies);

        var result = await new GetAllCurrenciesQueryHandler(_repository)
            .Handle(new GetAllCurrenciesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(r => r.Code).Should().BeEquivalentTo(["USD", "EUR"]);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([]);

        var result = await new GetAllCurrenciesQueryHandler(_repository)
            .Handle(new GetAllCurrenciesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturnResponse()
    {
        var id = Guid.NewGuid();
        var currency = Currency.Create("GBP", "British Pound", "£");
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(currency);

        var result = await new GetCurrencyByIdQueryHandler(_repository)
            .Handle(new GetCurrencyByIdQuery(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Code.Should().Be("GBP");
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Currency?)null);

        var result = await new GetCurrencyByIdQueryHandler(_repository)
            .Handle(new GetCurrencyByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
