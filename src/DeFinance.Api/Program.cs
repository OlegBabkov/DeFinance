using DeFinance.Application;
using DeFinance.Infrastructure;
using DeFinance.Infrastructure.Persistence;
using DeFinance.Infrastructure.Persistence.Seeders;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "DeFinance API", Version = "v1" });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeFinanceDbContext>();
    if (db.Database.IsRelational())
        await db.Database.MigrateAsync();
    await CurrencySeeder.SeedAsync(db);
    await AccountSeeder.SeedAsync(db);
    await CategorySeeder.SeedAsync(db);
    await CounterpartySeeder.SeedAsync(db);
    await PaymentStatusSeeder.SeedAsync(db);
    await UserSeeder.SeedAsync(db);
}

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var feature = context.Features.Get<IExceptionHandlerPathFeature>();

    if (feature?.Error is ValidationException ve)
    {
        var errors = ve.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new ValidationProblemDetails(errors) { Status = 400 });
        return;
    }

    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(
        new ProblemDetails { Status = 500, Title = "An unexpected error occurred." });
}));

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "DeFinance API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
