using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Counterparties.Queries;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Counterparties.Queries;

public class GetCounterpartyQueryHandlerTests
{
    private readonly ICounterpartyRepository _repository = Substitute.For<ICounterpartyRepository>();

    [Fact]
    public async Task GetAll_ShouldReturnMappedResponses()
    {
        var counterparties = new List<Counterparty>
        {
            Counterparty.Create("Lidl", CounterpartyType.Company, null),
            Counterparty.Create("Maria", CounterpartyType.Person, "maria@email.com")
        };
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(counterparties);

        var result = await new GetAllCounterpartiesQueryHandler(_repository)
            .Handle(new GetAllCounterpartiesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(r => r.Name).Should().BeEquivalentTo(["Lidl", "Maria"]);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnEmptyList()
    {
        _repository.GetAllAsync(Arg.Any<CancellationToken>()).Returns([]);

        var result = await new GetAllCounterpartiesQueryHandler(_repository)
            .Handle(new GetAllCounterpartiesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturnResponse()
    {
        var id = Guid.NewGuid();
        var counterparty = Counterparty.Create("Sparkasse", CounterpartyType.Company, null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(counterparty);

        var result = await new GetCounterpartyByIdQueryHandler(_repository)
            .Handle(new GetCounterpartyByIdQuery(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Sparkasse");
        result.Type.Should().Be(CounterpartyType.Company);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Counterparty?)null);

        var result = await new GetCounterpartyByIdQueryHandler(_repository)
            .Handle(new GetCounterpartyByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
