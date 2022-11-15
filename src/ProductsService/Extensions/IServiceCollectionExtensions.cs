using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProductsService.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, ProductsServiceConfiguration cfg)
    {
        //traces
        services.AddOpenTelemetryTracing(options =>
        {
            options
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Constants.ServiceName))
                .AddSource(Constants.ServiceName)
                .AddEntityFrameworkCoreInstrumentation(opt =>
                {
                    opt.SetDbStatementForText = true;
                    opt.SetDbStatementForStoredProcedure = true;
                })
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation();
    
            if (!string.IsNullOrWhiteSpace(cfg.ZipkinEndpoint))
            {
                options.AddZipkinExporter(config => config.Endpoint = new Uri(cfg.ZipkinEndpoint));
            }
        });

        // metrics
        services.AddOpenTelemetryMetrics(options => options
            .ConfigureResource(rb => rb.AddService(Constants.ServiceName))
            .AddRuntimeInstrumentation()
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddPrometheusExporter());

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection service) =>
        service.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo()
            {
                Version = "v1",
                Title = "Products Service",
                Description = "Fairly simple .NET API to interact with product data",
                Contact = new OpenApiContact
                {
                    Name = "Thinktecture AG",
                    Email = "info@thinktecture.com",
                    Url = new Uri("https://thinktecture.com")
                }
            });
        });
}