using System.Text;
using DeFinance.Api.BackgroundServices;
using DeFinance.Api.Hubs;
using DeFinance.Api.Observability;
using DeFinance.Application;
using DeFinance.Infrastructure;
using DeFinance.Infrastructure.Persistence;
using DeFinance.Infrastructure.Persistence.Seeders;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.AddStructuredLogging();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!)),
            ClockSkew = TimeSpan.Zero,
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var token = ctx.Request.Query["access_token"].ToString();
                if (!string.IsNullOrEmpty(token) && ctx.Request.Path.StartsWithSegments("/hubs"))
                    ctx.Token = token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddHostedService<TransactionEventSubscriber>();
builder.Services.AddOpenTelemetryObservability(builder.Configuration);
builder.Services.AddObservabilityHealthChecks(builder.Configuration);
builder.Services.AddControllers(o => o.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter()))
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

app.UseSerilogRequestLogging();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");
app.MapObservabilityEndpoints();

app.Run();

public partial class Program { }
