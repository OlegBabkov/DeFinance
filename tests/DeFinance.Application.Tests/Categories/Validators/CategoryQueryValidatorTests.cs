using DeFinance.Application.Categories.Queries;
using FluentAssertions;

namespace DeFinance.Application.Tests.Categories.Validators;

public class CategoryQueryValidatorTests
{
    private readonly GetAllCategoriesQueryValidator _validator = new();

    [Fact]
    public async Task Validator_WithDefaultQuery_ShouldBeValid()
    {
        var result = await _validator.ValidateAsync(new GetAllCategoriesQuery());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_WithInvalidPage_ShouldBeInvalid(int page)
    {
        var result = await _validator.ValidateAsync(new GetAllCategoriesQuery(Page: page));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllCategoriesQuery.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(501)]
    public async Task Validator_WithInvalidPageSize_ShouldBeInvalid(int pageSize)
    {
        var result = await _validator.ValidateAsync(new GetAllCategoriesQuery(PageSize: pageSize));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllCategoriesQuery.PageSize));
    }

    [Fact]
    public async Task Validator_WithNullSortBy_ShouldBeValid()
    {
        var result = await _validator.ValidateAsync(new GetAllCategoriesQuery(SortBy: null));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("name")]
    [InlineData("type")]
    [InlineData("NAME")]
    [InlineData("Type")]
    public async Task Validator_WithValidSortBy_ShouldBeValid(string sortBy)
    {
        var result = await _validator.ValidateAsync(new GetAllCategoriesQuery(SortBy: sortBy));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("color")]
    [InlineData("parentId")]
    public async Task Validator_WithInvalidSortBy_ShouldBeInvalid(string sortBy)
    {
        var result = await _validator.ValidateAsync(new GetAllCategoriesQuery(SortBy: sortBy));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllCategoriesQuery.SortBy));
    }
}
