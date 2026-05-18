using DeFinance.Application.Accounts.Queries;
using FluentAssertions;

namespace DeFinance.Application.Tests.Accounts.Validators;

public class AccountQueryValidatorTests
{
    private readonly GetAllAccountsQueryValidator _validator = new();

    [Fact]
    public async Task Validator_WithDefaultQuery_ShouldBeValid()
    {
        var result = await _validator.ValidateAsync(new GetAllAccountsQuery());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_WithInvalidPage_ShouldBeInvalid(int page)
    {
        var result = await _validator.ValidateAsync(new GetAllAccountsQuery(Page: page));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllAccountsQuery.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Validator_WithInvalidPageSize_ShouldBeInvalid(int pageSize)
    {
        var result = await _validator.ValidateAsync(new GetAllAccountsQuery(PageSize: pageSize));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllAccountsQuery.PageSize));
    }

    [Fact]
    public async Task Validator_WithNullSortBy_ShouldBeValid()
    {
        var result = await _validator.ValidateAsync(new GetAllAccountsQuery(SortBy: null));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("name")]
    [InlineData("type")]
    [InlineData("balance")]
    [InlineData("NAME")]
    [InlineData("Balance")]
    public async Task Validator_WithValidSortBy_ShouldBeValid(string sortBy)
    {
        var result = await _validator.ValidateAsync(new GetAllAccountsQuery(SortBy: sortBy));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("code")]
    [InlineData("currencyId")]
    public async Task Validator_WithInvalidSortBy_ShouldBeInvalid(string sortBy)
    {
        var result = await _validator.ValidateAsync(new GetAllAccountsQuery(SortBy: sortBy));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllAccountsQuery.SortBy));
    }
}
