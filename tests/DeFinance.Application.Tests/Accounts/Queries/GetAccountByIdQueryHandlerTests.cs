using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Accounts.Queries;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Accounts.Queries;

public class GetAccountByIdQueryHandlerTests
{
    private readonly IAccountRepository _repository = Substitute.For<IAccountRepository>();
    private readonly GetAccountByIdQueryHandler _handler;

    public GetAccountByIdQueryHandlerTests()
    {
        _handler = new GetAccountByIdQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenAccountExists_ShouldReturnResponse()
    {
        var id = Guid.NewGuid();
        var account = Account.Create("Checking Account", AccountType.Checking, 250m, Guid.NewGuid(), Guid.NewGuid());
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(account);

        var result = await _handler.Handle(new GetAccountByIdQuery(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Checking Account");
        result.Type.Should().Be(AccountType.Checking);
        result.Balance.Should().Be(250m);
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Account?)null);

        var result = await _handler.Handle(new GetAccountByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
