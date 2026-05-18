using System.Net;
using System.Net.Http.Json;
using DeFinance.Api.Tests.Infrastructure;
using DeFinance.Application.Common;
using DeFinance.Application.Counterparties.Commands;
using DeFinance.Application.DTOs.Counterparty;
using DeFinance.Domain.Entities;
using DeFinance.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DeFinance.Api.Tests.Controllers;

public class CounterpartiesControllerTests : IClassFixture<DeFinanceWebApplicationFactory>, IAsyncLifetime
{
    private readonly DeFinanceWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CounterpartiesControllerTests(DeFinanceWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DeFinanceDbContext>();
        db.Counterparties.RemoveRange(db.Counterparties);
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_Initially_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/counterparties");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<PagedResult<CounterpartyResponse>>(DeFinanceWebApplicationFactory.JsonOptions);
        body.Should().NotBeNull();
        body!.Items.Should().BeEmpty();
        body.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        var command = new CreateCounterpartyCommand("Lidl", CounterpartyType.Company, null);

        var response = await _client.PostAsJsonAsync("/api/counterparties", command, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CounterpartyResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body.Should().NotBeNull();
        body!.Name.Should().Be("Lidl");
        body.Type.Should().Be(CounterpartyType.Company);
        body.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        var command = new CreateCounterpartyCommand("", CounterpartyType.Company, null);

        var response = await _client.PostAsJsonAsync("/api/counterparties", command, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithNameTooLong_ReturnsBadRequest()
    {
        var command = new CreateCounterpartyCommand(new string('A', 101), CounterpartyType.Company, null);

        var response = await _client.PostAsJsonAsync("/api/counterparties", command, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsCounterparty()
    {
        var created = await CreateCounterpartyAsync("Rewe", CounterpartyType.Company, null);

        var response = await _client.GetAsync($"/api/counterparties/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CounterpartyResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.Id.Should().Be(created.Id);
        body.Name.Should().Be("Rewe");
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/counterparties/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenExists_ReturnsUpdatedCounterparty()
    {
        var created = await CreateCounterpartyAsync("Amazon", CounterpartyType.Company, null);
        var update = new UpdateCounterpartyCommand(created.Id, "Amazon DE", CounterpartyType.Company, "contact@amazon.de");

        var response = await _client.PutAsJsonAsync($"/api/counterparties/{created.Id}", update, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CounterpartyResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.Name.Should().Be("Amazon DE");
        body.ContactInfo.Should().Be("contact@amazon.de");
    }

    [Fact]
    public async Task Update_WithEmptyName_ReturnsBadRequest()
    {
        var created = await CreateCounterpartyAsync("Edeka", CounterpartyType.Company, null);
        var update = new UpdateCounterpartyCommand(created.Id, "", CounterpartyType.Company, null);

        var response = await _client.PutAsJsonAsync($"/api/counterparties/{created.Id}", update, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        var update = new UpdateCounterpartyCommand(Guid.NewGuid(), "Ghost", CounterpartyType.Other, null);

        var response = await _client.PutAsJsonAsync($"/api/counterparties/{Guid.NewGuid()}", update, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WhenActive_ReturnsInactiveCounterparty()
    {
        var created = await CreateCounterpartyAsync("Shell", CounterpartyType.Company, null);

        var response = await _client.PatchAsync($"/api/counterparties/{created.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CounterpartyResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Activate_WhenInactive_ReturnsActiveCounterparty()
    {
        var created = await CreateCounterpartyAsync("ING", CounterpartyType.Company, null);
        await _client.PatchAsync($"/api/counterparties/{created.Id}/deactivate", null);

        var response = await _client.PatchAsync($"/api/counterparties/{created.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CounterpartyResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WhenNotFound_ReturnsNotFound()
    {
        var response = await _client.PatchAsync($"/api/counterparties/{Guid.NewGuid()}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_PersonType_ReturnsCorrectType()
    {
        var command = new CreateCounterpartyCommand("Maria", CounterpartyType.Person, "maria@email.com");

        var response = await _client.PostAsJsonAsync("/api/counterparties", command, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CounterpartyResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.Type.Should().Be(CounterpartyType.Person);
        body.ContactInfo.Should().Be("maria@email.com");
    }

    private async Task<CounterpartyResponse> CreateCounterpartyAsync(string name, CounterpartyType type, string? contactInfo)
    {
        var response = await _client.PostAsJsonAsync("/api/counterparties", new CreateCounterpartyCommand(name, type, contactInfo), DeFinanceWebApplicationFactory.JsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CounterpartyResponse>(DeFinanceWebApplicationFactory.JsonOptions))!;
    }
}
