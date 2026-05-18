using System.Net;
using System.Net.Http.Json;
using DeFinance.Api.Tests.Infrastructure;
using DeFinance.Application.Common;
using DeFinance.Application.Currencies.Commands;
using DeFinance.Application.DTOs.Currency;
using DeFinance.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DeFinance.Api.Tests.Controllers;

public class CurrenciesControllerTests : IClassFixture<DeFinanceWebApplicationFactory>, IAsyncLifetime
{
    private readonly DeFinanceWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CurrenciesControllerTests(DeFinanceWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DeFinanceDbContext>();
        db.Currencies.RemoveRange(db.Currencies);
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_Initially_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/currencies");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<CurrencyResponse>>();
        body.Should().NotBeNull();
        body!.Items.Should().BeEmpty();
        body.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        var command = new CreateCurrencyCommand("EUR", "Euro", "€");

        var response = await _client.PostAsJsonAsync("/api/currencies", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CurrencyResponse>();
        body.Should().NotBeNull();
        body!.Code.Should().Be("EUR");
        body.Name.Should().Be("Euro");
        body.Symbol.Should().Be("€");
        body.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithEmptyCode_ReturnsBadRequest()
    {
        var command = new CreateCurrencyCommand("", "Euro", "€");

        var response = await _client.PostAsJsonAsync("/api/currencies", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithNumericCode_ReturnsBadRequest()
    {
        var command = new CreateCurrencyCommand("123", "Invalid", "?");

        var response = await _client.PostAsJsonAsync("/api/currencies", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsCurrency()
    {
        var created = await CreateCurrencyAsync("GBP", "British Pound", "£");

        var response = await _client.GetAsync($"/api/currencies/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CurrencyResponse>();
        body!.Id.Should().Be(created.Id);
        body.Code.Should().Be("GBP");
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/currencies/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenExists_ReturnsUpdatedCurrency()
    {
        var created = await CreateCurrencyAsync("JPY", "Japanese Yen", "¥");
        var updateCommand = new UpdateCurrencyCommand(created.Id, "Yen", "¥¥");

        var response = await _client.PutAsJsonAsync($"/api/currencies/{created.Id}", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CurrencyResponse>();
        body!.Name.Should().Be("Yen");
        body.Symbol.Should().Be("¥¥");
    }

    [Fact]
    public async Task Update_WithEmptyName_ReturnsBadRequest()
    {
        var created = await CreateCurrencyAsync("CHF", "Swiss Franc", "Fr");
        var updateCommand = new UpdateCurrencyCommand(created.Id, "", "Fr");

        var response = await _client.PutAsJsonAsync($"/api/currencies/{created.Id}", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        var updateCommand = new UpdateCurrencyCommand(Guid.NewGuid(), "Ghost", "?");

        var response = await _client.PutAsJsonAsync($"/api/currencies/{Guid.NewGuid()}", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WhenActive_ReturnsInactiveCurrency()
    {
        var created = await CreateCurrencyAsync("CAD", "Canadian Dollar", "C$");

        var response = await _client.PatchAsync($"/api/currencies/{created.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CurrencyResponse>();
        body!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Activate_WhenInactive_ReturnsActiveCurrency()
    {
        var created = await CreateCurrencyAsync("AUD", "Australian Dollar", "A$");
        await _client.PatchAsync($"/api/currencies/{created.Id}/deactivate", null);

        var response = await _client.PatchAsync($"/api/currencies/{created.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CurrencyResponse>();
        body!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WhenNotFound_ReturnsNotFound()
    {
        var response = await _client.PatchAsync($"/api/currencies/{Guid.NewGuid()}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<CurrencyResponse> CreateCurrencyAsync(string code, string name, string symbol)
    {
        var response = await _client.PostAsJsonAsync("/api/currencies", new CreateCurrencyCommand(code, name, symbol));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CurrencyResponse>())!;
    }
}
