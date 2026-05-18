using DeFinance.Application.PaymentStatuses.Queries;
using FluentAssertions;

namespace DeFinance.Application.Tests.PaymentStatuses.Validators;

public class PaymentStatusQueryValidatorTests
{
    private readonly GetAllPaymentStatusesQueryValidator _validator = new();

    [Fact]
    public async Task Validator_WithDefaultQuery_ShouldBeValid()
    {
        var result = await _validator.ValidateAsync(new GetAllPaymentStatusesQuery());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_WithInvalidPage_ShouldBeInvalid(int page)
    {
        var result = await _validator.ValidateAsync(new GetAllPaymentStatusesQuery(Page: page));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllPaymentStatusesQuery.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Validator_WithInvalidPageSize_ShouldBeInvalid(int pageSize)
    {
        var result = await _validator.ValidateAsync(new GetAllPaymentStatusesQuery(PageSize: pageSize));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllPaymentStatusesQuery.PageSize));
    }

    [Fact]
    public async Task Validator_WithNullSortBy_ShouldBeValid()
    {
        var result = await _validator.ValidateAsync(new GetAllPaymentStatusesQuery(SortBy: null));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("name")]
    [InlineData("NAME")]
    [InlineData("Name")]
    public async Task Validator_WithValidSortBy_ShouldBeValid(string sortBy)
    {
        var result = await _validator.ValidateAsync(new GetAllPaymentStatusesQuery(SortBy: sortBy));
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("description")]
    [InlineData("isActive")]
    public async Task Validator_WithInvalidSortBy_ShouldBeInvalid(string sortBy)
    {
        var result = await _validator.ValidateAsync(new GetAllPaymentStatusesQuery(SortBy: sortBy));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GetAllPaymentStatusesQuery.SortBy));
    }
}
