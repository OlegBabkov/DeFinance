using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Counterparties.Commands;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Counterparties.Commands;

public class CreateCounterpartyCommandHandlerTests
{
    private readonly ICounterpartyRepository _repository = Substitute.For<ICounterpartyRepository>();
    private readonly CreateCounterpartyCommandHandler _handler;

    public CreateCounterpartyCommandHandlerTests()
    {
        _handler = new CreateCounterpartyCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldCreateCounterpartyAndReturnResponse()
    {
        var command = new CreateCounterpartyCommand("Lidl", CounterpartyType.Company, "info@lidl.de");
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Lidl");
        result.Type.Should().Be(CounterpartyType.Company);
        result.ContactInfo.Should().Be("info@lidl.de");
        result.IsActive.Should().BeTrue();

        await _repository.Received(1).AddAsync(Arg.Any<Counterparty>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNullContactInfo_ShouldCreateSuccessfully()
    {
        var command = new CreateCounterpartyCommand("Maria", CounterpartyType.Person, null);
        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Maria");
        result.ContactInfo.Should().BeNull();
    }
}
