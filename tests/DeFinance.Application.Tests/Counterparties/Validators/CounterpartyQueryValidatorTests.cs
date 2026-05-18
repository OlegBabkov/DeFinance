using DeFinance.Application.Counterparties.Queries;
using FluentAssertions;

namespace DeFinance.Application.Tests.Counterparties.Validators;

public class CounterpartyQueryValidatorTests
{
    private readonly GetAllCounterpartiesQueryValidator _validator = new();

    [Fact]
    public async Task Validator_WithDefaultQuery_ShouldBeValid()
    {
        var result = await _validator.ValidateAsync(new GetAllCounterpartiesQuery());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_WithInvalidPage_ShouldBeInvalid(int page)
    {
        var result = await _validator.ValidateAsync(new GetAllCounterpartiesQuery(Page: page));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllCounterpartiesQuery.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Validator_WithInvalidPageSize_ShouldBeInvalid(int pageSize)
    {
        var result = await _validator.ValidateAsync(new GetAllCounterpartiesQuery(PageSize: pageSize));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllCounterpartiesQuery.PageSize));
    }

    [Fact]
    public async Task Validator_WithNullSortBy_ShouldBeValid()
    {
        var result = await _validator.ValidateAsync(new GetAllCounterpartiesQuery(SortBy: null));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("name")]
    [InlineData("type")]
    [InlineData("NAME")]
    [InlineData("Type")]
    public async Task Validator_WithValidSortBy_ShouldBeValid(string sortBy)
    {
        var result = await _validator.ValidateAsync(new GetAllCounterpartiesQuery(SortBy: sortBy));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("contactInfo")]
    [InlineData("id")]
    public async Task Validator_WithInvalidSortBy_ShouldBeInvalid(string sortBy)
    {
        var result = await _validator.ValidateAsync(new GetAllCounterpartiesQuery(SortBy: sortBy));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllCounterpartiesQuery.SortBy));
    }
}
