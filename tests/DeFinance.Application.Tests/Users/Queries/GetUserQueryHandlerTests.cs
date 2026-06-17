using DeFinance.Application.Abstractions.Repositories;
using DeFinance.Application.Common;
using DeFinance.Application.Users.Queries;
using DeFinance.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace DeFinance.Application.Tests.Users.Queries;

public class GetUserQueryHandlerTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();

    private void SetupGetAll(IReadOnlyList<User> items, int totalCount) =>
        _repository.GetAllAsync(
            Arg.Any<string?>(), Arg.Any<bool?>(),
            Arg.Any<string?>(), Arg.Any<SortDirection>(),
            Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<CancellationToken>())
        .Returns((items, totalCount));

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_WhenUserExists_ShouldReturnResponse()
    {
        var id = Guid.NewGuid();
        var user = User.Create("alice", "pw", "alice@example.com", null);
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(user);

        var result = await new GetUserByIdQueryHandler(_repository)
            .Handle(new GetUserByIdQuery(id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Username.Should().Be("alice");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetById_WhenUserNotFound_ShouldReturnNull()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await new GetUserByIdQueryHandler(_repository)
            .Handle(new GetUserByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturnPagedResultWithMappedItems()
    {
        var users = new List<User>
        {
            User.Create("alice", "pw1", "alice@example.com", null),
            User.Create("bob", "pw2", "bob@example.com", null),
        };
        SetupGetAll(users, 2);

        var result = await new GetAllUsersQueryHandler(_repository)
            .Handle(new GetAllUsersQuery(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Select(r => r.Username).Should().BeEquivalentTo(["alice", "bob"]);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnEmptyPagedResult()
    {
        SetupGetAll([], 0);

        var result = await new GetAllUsersQueryHandler(_repository)
            .Handle(new GetAllUsersQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_ShouldPassFiltersToRepository()
    {
        SetupGetAll([], 0);

        await new GetAllUsersQueryHandler(_repository)
            .Handle(new GetAllUsersQuery(Search: "ali", IsActive: true, Page: 2, PageSize: 10, SortBy: "username", SortDirection: SortDirection.Desc),
            CancellationToken.None);

        await _repository.Received(1).GetAllAsync(
            "ali", true, "username", SortDirection.Desc, 2, 10, Arg.Any<CancellationToken>());
    }
}
