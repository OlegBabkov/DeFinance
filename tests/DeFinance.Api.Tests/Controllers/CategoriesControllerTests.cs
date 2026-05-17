using System.Net;
using System.Net.Http.Json;
using DeFinance.Api.Tests.Infrastructure;
using DeFinance.Application.Categories.Commands;
using DeFinance.Application.DTOs.Category;
using DeFinance.Domain.Entities;
using DeFinance.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DeFinance.Api.Tests.Controllers;

public class CategoriesControllerTests : IClassFixture<DeFinanceWebApplicationFactory>, IAsyncLifetime
{
    private readonly DeFinanceWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CategoriesControllerTests(DeFinanceWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DeFinanceDbContext>();
        db.Categories.RemoveRange(db.Categories);
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_Initially_ReturnsEmptyList()
    {
        var response = await _client.GetAsync("/api/categories");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>(DeFinanceWebApplicationFactory.JsonOptions);
        body.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        var command = new CreateCategoryCommand("Food", CategoryType.Expense, "#FF5733", "🍔", null);

        var response = await _client.PostAsJsonAsync("/api/categories", command, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CategoryResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body.Should().NotBeNull();
        body!.Name.Should().Be("Food");
        body.Type.Should().Be(CategoryType.Expense);
        body.Color.Should().Be("#FF5733");
        body.Icon.Should().Be("🍔");
        body.ParentId.Should().BeNull();
        body.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_WithNoOptionals_ReturnsCreated()
    {
        var command = new CreateCategoryCommand("Salary", CategoryType.Income, null, null, null);

        var response = await _client.PostAsJsonAsync("/api/categories", command, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CategoryResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.Color.Should().BeNull();
        body.Icon.Should().BeNull();
    }

    [Fact]
    public async Task Create_WithEmptyName_ReturnsBadRequest()
    {
        var command = new CreateCategoryCommand("", CategoryType.Expense, null, null, null);

        var response = await _client.PostAsJsonAsync("/api/categories", command, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithInvalidColor_ReturnsBadRequest()
    {
        var command = new CreateCategoryCommand("Food", CategoryType.Expense, "red", null, null);

        var response = await _client.PostAsJsonAsync("/api/categories", command, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithParentId_ReturnsCreatedWithParent()
    {
        var parent = await CreateCategoryAsync("Food", CategoryType.Expense, null);
        var command = new CreateCategoryCommand("Fast Food", CategoryType.Expense, null, null, parent.Id);

        var response = await _client.PostAsJsonAsync("/api/categories", command, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CategoryResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.ParentId.Should().Be(parent.Id);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsCategory()
    {
        var created = await CreateCategoryAsync("Transport", CategoryType.Expense, "#0000FF");

        var response = await _client.GetAsync($"/api/categories/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CategoryResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.Id.Should().Be(created.Id);
        body.Name.Should().Be("Transport");
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/categories/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenExists_ReturnsUpdatedCategory()
    {
        var created = await CreateCategoryAsync("Old Name", CategoryType.Income, null);
        var updateCommand = new UpdateCategoryCommand(created.Id, "New Name", "#AABBCC", "💰");

        var response = await _client.PutAsJsonAsync($"/api/categories/{created.Id}", updateCommand, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CategoryResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.Name.Should().Be("New Name");
        body.Color.Should().Be("#AABBCC");
        body.Icon.Should().Be("💰");
    }

    [Fact]
    public async Task Update_WithEmptyName_ReturnsBadRequest()
    {
        var created = await CreateCategoryAsync("Category", CategoryType.Expense, null);
        var updateCommand = new UpdateCategoryCommand(created.Id, "", null, null);

        var response = await _client.PutAsJsonAsync($"/api/categories/{created.Id}", updateCommand, DeFinanceWebApplicationFactory.JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenNotFound_ReturnsNotFound()
    {
        var updateCommand = new UpdateCategoryCommand(Guid.NewGuid(), "Ghost", null, null);

        var response = await _client.PutAsJsonAsync($"/api/categories/{Guid.NewGuid()}", updateCommand);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deactivate_WhenActive_ReturnsInactiveCategory()
    {
        var created = await CreateCategoryAsync("Shopping", CategoryType.Expense, null);

        var response = await _client.PatchAsync($"/api/categories/{created.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CategoryResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Activate_WhenInactive_ReturnsActiveCategory()
    {
        var created = await CreateCategoryAsync("Rent", CategoryType.Expense, null);
        await _client.PatchAsync($"/api/categories/{created.Id}/deactivate", null);

        var response = await _client.PatchAsync($"/api/categories/{created.Id}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CategoryResponse>(DeFinanceWebApplicationFactory.JsonOptions);
        body!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Activate_WhenNotFound_ReturnsNotFound()
    {
        var response = await _client.PatchAsync($"/api/categories/{Guid.NewGuid()}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<CategoryResponse> CreateCategoryAsync(string name, CategoryType type, string? color)
    {
        var response = await _client.PostAsJsonAsync("/api/categories",
            new CreateCategoryCommand(name, type, color, null, null));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CategoryResponse>(DeFinanceWebApplicationFactory.JsonOptions))!;
    }
}
