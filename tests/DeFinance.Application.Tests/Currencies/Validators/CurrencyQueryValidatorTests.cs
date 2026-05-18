using DeFinance.Application.Currencies.Queries;
using FluentAssertions;

namespace DeFinance.Application.Tests.Currencies.Validators;

public class CurrencyQueryValidatorTests
{
    private readonly GetAllCurrenciesQueryValidator _validator = new();

    [Fact]
    public async Task Validator_WithDefaultQuery_ShouldBeValid()
    {
        var result = await _validator.ValidateAsync(new GetAllCurrenciesQuery());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_WithInvalidPage_ShouldBeInvalid(int page)
    {
        var result = await _validator.ValidateAsync(new GetAllCurrenciesQuery(Page: page));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllCurrenciesQuery.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Validator_WithInvalidPageSize_ShouldBeInvalid(int pageSize)
    {
        var result = await _validator.ValidateAsync(new GetAllCurrenciesQuery(PageSize: pageSize));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllCurrenciesQuery.PageSize));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(50)]
    public async Task Validator_WithValidPageSize_ShouldBeValid(int pageSize)
    {
        var result = await _validator.ValidateAsync(new GetAllCurrenciesQuery(PageSize: pageSize));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_WithNullSortBy_ShouldBeValid()
    {
        var result = await _validator.ValidateAsync(new GetAllCurrenciesQuery(SortBy: null));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("name")]
    [InlineData("code")]
    [InlineData("symbol")]
    [InlineData("NAME")]
    [InlineData("Code")]
    public async Task Validator_WithValidSortBy_ShouldBeValid(string sortBy)
    {
        var result = await _validator.ValidateAsync(new GetAllCurrenciesQuery(SortBy: sortBy));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("id")]
    [InlineData("isActive")]
    [InlineData("createdAt")]
    public async Task Validator_WithInvalidSortBy_ShouldBeInvalid(string sortBy)
    {
        var result = await _validator.ValidateAsync(new GetAllCurrenciesQuery(SortBy: sortBy));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllCurrenciesQuery.SortBy));
    }
}
