using System.Net;
using System.Net.Http.Json;
using DeFinance.Api.Tests.Infrastructure;
using DeFinance.Application.DTOs.PaymentStatus;
using DeFinance.Application.PaymentStatuses.Commands;
using DeFinance.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DeFinance.Api.Tests.Controllers;

public class PaymentStatusesControllerTests : IClassFixture<DeFinanceWebApplicationFactory>, IAsyncLifetime
{
    private readonly DeFinanceWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PaymentStatusesControllerTests(DeFinanceWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DeFinanceDbContext>();
        db.PaymentStatuses.RemoveRange(db.PaymentStatuses);
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_Initially_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/payment-statuses");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<PaymentStatusResponse>>();
        body.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        var command = new CreatePaymentStatusCommand("Paid", "Transaction fully paid.");

        var response = await _client.PostAsJsonAsync("/api/payment-statuses", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<PaymentStatusResponse>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Paid");
        body.Description.Should().Be("Transaction fully paid.");
        body.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        var command = new CreatePaymentStatusCommand("", null);

        var response = await _client.PostAsJsonAsync("/api/payment-statuses", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithNameTooLong_ReturnsBadRequest()
    {
        var command = new CreatePaymentStatusCommand(new string('A', 101), null);

        var response = await _client.PostAsJsonAsync("/api/payment-statuses", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsPaymentStatus()
    {
        var created = await CreateStatusAsync("Reserved", "Funds reserved.");

        var response = await _client.GetAsync($"/api/payment-statuses/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PaymentStatusResponse>();
        body!.Id.Should().Be(created.Id);
        body.Name.Should().Be("Reserved");
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/payment-statuses/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenExists_ReturnsUpdatedPaymentStatus()
    {
        var created = await CreateStatusAsync("Booked", null);
        var update = new UpdatePaymentStatusCommand(created.Id, "Booked", "Confirmed and booked.");

        var response = await _client.PutAsJsonAsync($"/api/payment-statuses/{created.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PaymentStatusResponse>();
        body!.Description.Should().Be("Confirmed and booked.");
    }

    [Fact]
    public async Task Update_WithEmptyName_ReturnsBadRequest()
    {
        var created = await CreateStatusAsync("Rejected", null);
        var update = new UpdatePaymentStatusCommand(created.Id, "", null);

        var response = await _client.PutAsJsonAsync($"/api/payment-statuses/{created.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        var update = new UpdatePaymentStatusCommand(Guid.NewGuid(), "Ghost", null);

        var response = await _client.PutAsJsonAsync($"/api/payment-statuses/{Guid.NewGuid()}", update);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WhenActive_ReturnsInactiveStatus()
    {
        var created = await CreateStatusAsync("Paid", null);

        var response = await _client.PatchAsync($"/api/payment-statuses/{created.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PaymentStatusResponse>();
        body!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Activate_WhenInactive_ReturnsActiveStatus()
    {
        var created = await CreateStatusAsync("Rejected", null);
        await _client.PatchAsync($"/api/payment-statuses/{created.Id}/deactivate", null);

        var response = await _client.PatchAsync($"/api/payment-statuses/{created.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PaymentStatusResponse>();
        body!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WhenNotFound_ReturnsNotFound()
    {
        var response = await _client.PatchAsync($"/api/payment-statuses/{Guid.NewGuid()}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<PaymentStatusResponse> CreateStatusAsync(string name, string? description)
    {
        var response = await _client.PostAsJsonAsync("/api/payment-statuses", new CreatePaymentStatusCommand(name, description));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PaymentStatusResponse>())!;
    }
}
