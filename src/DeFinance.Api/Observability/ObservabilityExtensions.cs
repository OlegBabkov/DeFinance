using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Reflection;

namespace DeFinance.Api.Observability;

public static class ObservabilityExtensions
{
    public static IHostBuilder AddStructuredLogging(this IHostBuilder host) =>
        host.UseSerilog((ctx, services, lc) =>
        {
            var env = ctx.HostingEnvironment.EnvironmentName.ToLower();
            var esUrl = ctx.Configuration["Observability:ElasticsearchUrl"] ?? "http://localhost:9200";
            var esUsername = ctx.Configuration["Observability:ElasticsearchUsername"];
            var esPassword = ctx.Configuration["Observability:ElasticsearchPassword"];

            lc
                .ReadFrom.Configuration(ctx.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(
                    new[] { new Uri(esUrl) },
                    opts =>
                    {
                        opts.DataStream = new DataStreamName("logs", "definance", env);
                        opts.BootstrapMethod = BootstrapMethod.Silent;
                    },
                    configureTransport: tc =>
                    {
                        if (!string.IsNullOrWhiteSpace(esUsername))
                            tc.Authentication(new Elastic.Transport.BasicAuthentication(esUsername, esPassword!));
                    });
        });

    public static IServiceCollection AddOpenTelemetryObservability(
        this IServiceCollection services, IConfiguration configuration)
    {
        var otlpEndpoint = configuration["Observability:OtlpEndpoint"] ?? "http://localhost:4317";
        var serviceName   = configuration["Observability:ServiceName"] ?? "definance-api";
        var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development",
                }))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(otlpEndpoint);
                    o.Protocol = OtlpExportProtocol.Grpc;
                }))
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)));

        return services;
    }

    public static IServiceCollection AddObservabilityHealthChecks(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;

        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgres", tags: ["ready", "db"])
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

        return services;
    }

    public static IEndpointRouteBuilder MapObservabilityEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = c => c.Tags.Contains("live"),
            ResultStatusCodes =
            {
                [HealthStatus.Healthy]   = StatusCodes.Status200OK,
                [HealthStatus.Degraded]  = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
            },
        });
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = c => c.Tags.Contains("ready"),
            ResultStatusCodes =
            {
                [HealthStatus.Healthy]   = StatusCodes.Status200OK,
                [HealthStatus.Degraded]  = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
            },
        });

        return app;
    }
}
