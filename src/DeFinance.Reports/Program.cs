using DeFinance.Infrastructure.Persistence;
using DeFinance.Reports.Consumers;
using DeFinance.Reports.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using StackExchange.Redis;

QuestPDF.Settings.License = LicenseType.Community;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<DeFinanceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!));

builder.Services.AddScoped<IPdfReportService, PdfReportService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<GenerateReportConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var rmqHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        var rmqUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var rmqPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(rmqHost, h =>
        {
            h.Username(rmqUser);
            h.Password(rmqPass);
        });

        cfg.ConfigureEndpoints(ctx);
    });
});

var app = builder.Build();
app.Run();
