using System.Net;
using System.Net.Http.Json;
using DeFinance.Api.Tests.Infrastructure;
using DeFinance.Application.Accounts.Commands;
using DeFinance.Application.Common;
using DeFinance.Application.DTOs.Account;
using DeFinance.Domain.Entities;
using DeFinance.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DeFinance.Api.Tests.Controllers;

public class AccountsControllerTests : IClassFixture<DeFinanceWebApplicationFactory>, IAsyncLifetime
{
    private readonly DeFinanceWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private Guid _currencyId;

    public AccountsControllerTests(DeFinanceWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DeFinanceDbContext>();
        db.Accounts.RemoveRange(db.Accounts);
        await db.SaveChangesAsync();
        _currencyId = await db.Currencies.Select(c => c.Id).FirstAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_Initially_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/accounts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<AccountResponse>>(DeFinanceWebApplicationFactory.JsonOptions);
        body.Should().NotBeNull();
        body!.Items.Should().BeEmpty();
        body.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        var command = new CreateAccountCommand("My Savings", AccountType.Savings, 500m, _currencyId);

        var response = await _client.PostAsJsonAsync("/api/accounts", command, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AccountResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body.Should().NotBeNull();
        body!.Name.Should().Be("My Savings");
        body.Type.Should().Be(AccountType.Savings);
        body.Balance.Should().Be(500m);
        body.CurrencyId.Should().Be(_currencyId);
        body.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        var command = new CreateAccountCommand("", AccountType.Checking, 0m, _currencyId);

        var response = await _client.PostAsJsonAsync("/api/accounts", command, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithNegativeBalance_ReturnsBadRequest()
    {
        var command = new CreateAccountCommand("Account", AccountType.Checking, -1m, _currencyId);

        var response = await _client.PostAsJsonAsync("/api/accounts", command, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithEmptyCurrencyId_ReturnsBadRequest()
    {
        var command = new CreateAccountCommand("Account", AccountType.Checking, 0m, Guid.Empty);

        var response = await _client.PostAsJsonAsync("/api/accounts", command, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsAccount()
    {
        var created = await CreateAccountAsync("Checking Account", AccountType.Checking, 100m);

        var response = await _client.GetAsync($"/api/accounts/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AccountResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.Id.Should().Be(created.Id);
        body.Name.Should().Be("Checking Account");
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/accounts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenExists_ReturnsUpdatedAccount()
    {
        var created = await CreateAccountAsync("Old Name", AccountType.Cash, 0m);
        var updateCommand = new UpdateAccountCommand(created.Id, "New Name");

        var response = await _client.PutAsJsonAsync($"/api/accounts/{created.Id}", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AccountResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task Update_WithEmptyName_ReturnsBadRequest()
    {
        var created = await CreateAccountAsync("Account", AccountType.Savings, 0m);
        var updateCommand = new UpdateAccountCommand(created.Id, "");

        var response = await _client.PutAsJsonAsync($"/api/accounts/{created.Id}", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        var updateCommand = new UpdateAccountCommand(Guid.NewGuid(), "Ghost");

        var response = await _client.PutAsJsonAsync($"/api/accounts/{Guid.NewGuid()}", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WhenActive_ReturnsInactiveAccount()
    {
        var created = await CreateAccountAsync("Investment", AccountType.Investment, 10000m);

        var response = await _client.PatchAsync($"/api/accounts/{created.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AccountResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Activate_WhenInactive_ReturnsActiveAccount()
    {
        var created = await CreateAccountAsync("Credit Card", AccountType.Credit, 0m);
        await _client.PatchAsync($"/api/accounts/{created.Id}/deactivate", null);

        var response = await _client.PatchAsync($"/api/accounts/{created.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AccountResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WhenNotFound_ReturnsNotFound()
    {
        var response = await _client.PatchAsync($"/api/accounts/{Guid.NewGuid()}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_WithMultipleItems_ReturnsCorrectPagedMetadata()
    {
        await CreateAccountAsync("Checking", AccountType.Checking, 100m);
        await CreateAccountAsync("Savings", AccountType.Savings, 500m);
        await CreateAccountAsync("Investment", AccountType.Investment, 1000m);

        var response = await _client.GetAsync("/api/accounts");
        var body = await response.Content.ReadFromJsonAsync<PagedResult<AccountResponse>>(DeFinanceWebApplicationFactory.JsonOptions);

        body!.TotalCount.Should().Be(3);
        body.Page.Should().Be(1);
        body.TotalPages.Should().Be(1);
        body.HasNextPage.Should().BeFalse();
        body.HasPreviousPage.Should().BeFalse();
        body.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAll_WithPageSize1_ReturnsSingleItemAndCorrectTotalPages()
    {
        await CreateAccountAsync("Checking", AccountType.Checking, 0m);
        await CreateAccountAsync("Savings", AccountType.Savings, 0m);

        var response = await _client.GetAsync("/api/accounts?page=1&pageSize=1");
        var body = await response.Content.ReadFromJsonAsync<PagedResult<AccountResponse>>(DeFinanceWebApplicationFactory.JsonOptions);

        body!.Items.Should().HaveCount(1);
        body.TotalCount.Should().Be(2);
        body.TotalPages.Should().Be(2);
        body.HasNextPage.Should().BeTrue();
        body.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetAll_WithSearchFilter_ReturnsOnlyMatchingItems()
    {
        await CreateAccountAsync("My Savings", AccountType.Savings, 0m);
        await CreateAccountAsync("Credit Card", AccountType.Credit, 0m);

        var response = await _client.GetAsync("/api/accounts?search=savings");
        var body = await response.Content.ReadFromJsonAsync<PagedResult<AccountResponse>>(DeFinanceWebApplicationFactory.JsonOptions);

        body!.TotalCount.Should().Be(1);
        body.Items.Single().Name.Should().Be("My Savings");
    }

    [Fact]
    public async Task GetAll_WithTypeFilter_ReturnsOnlyMatchingAccounts()
    {
        await CreateAccountAsync("Checking 1", AccountType.Checking, 0m);
        await CreateAccountAsync("Checking 2", AccountType.Checking, 0m);
        await CreateAccountAsync("Savings 1", AccountType.Savings, 0m);

        var response = await _client.GetAsync("/api/accounts?type=Checking");
        var body = await response.Content.ReadFromJsonAsync<PagedResult<AccountResponse>>(DeFinanceWebApplicationFactory.JsonOptions);

        body!.TotalCount.Should().Be(2);
        body.Items.Should().AllSatisfy(a => a.Type.Should().Be(AccountType.Checking));
    }

    [Fact]
    public async Task GetAll_WithIsActiveFilter_ReturnsOnlyInactiveAccounts()
    {
        var created = await CreateAccountAsync("Old Account", AccountType.Cash, 0m);
        await CreateAccountAsync("Active Account", AccountType.Checking, 0m);
        await _client.PatchAsync($"/api/accounts/{created.Id}/deactivate", null);

        var response = await _client.GetAsync("/api/accounts?isActive=false");
        var body = await response.Content.ReadFromJsonAsync<PagedResult<AccountResponse>>(DeFinanceWebApplicationFactory.JsonOptions);

        body!.TotalCount.Should().Be(1);
        body.Items.Single().Name.Should().Be("Old Account");
    }

    [Fact]
    public async Task GetAll_WithSortByBalance_ReturnsItemsInAscendingOrder()
    {
        await CreateAccountAsync("High", AccountType.Savings, 1000m);
        await CreateAccountAsync("Low", AccountType.Savings, 10m);
        await CreateAccountAsync("Mid", AccountType.Savings, 500m);

        var response = await _client.GetAsync("/api/accounts?sortBy=balance&sortDirection=Asc");
        var body = await response.Content.ReadFromJsonAsync<PagedResult<AccountResponse>>(DeFinanceWebApplicationFactory.JsonOptions);

        body!.Items.Select(a => a.Balance).Should().BeInAscendingOrder();
    }

    [Theory]
    [InlineData("/api/accounts?page=0")]
    [InlineData("/api/accounts?pageSize=0")]
    [InlineData("/api/accounts?pageSize=101")]
    [InlineData("/api/accounts?sortBy=invalid")]
    public async Task GetAll_WithInvalidQueryParams_ReturnsBadRequest(string url)
    {
        var response = await _client.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<AccountResponse> CreateAccountAsync(string name, AccountType type, decimal balance)
    {
        var response = await _client.PostAsJsonAsync("/api/accounts",
            new CreateAccountCommand(name, type, balance, _currencyId), DeFinanceWebApplicationFactory.JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AccountResponse>(DeFinanceWebApplicationFactory.JsonOptions))!;
    }
}
