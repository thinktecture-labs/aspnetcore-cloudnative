using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProductsService.Configuration;
using ProductsService.Data;
using ProductsService.Data.Entities;

const string ServiceName = "ProductsService";

var builder = WebApplication.CreateBuilder(args);

var cfg = new ProductsServiceConfiguration();
var cfgSection = builder.Configuration.GetSection(ProductsServiceConfiguration.SectionName);

if (cfgSection == null || !cfgSection.Exists())
{
    throw new ApplicationException(
        $"Could not find service config. Please provide a '{ProductsServiceConfiguration.SectionName}' config section");
}
cfgSection.Bind(cfg);
builder.Services.AddDbContext<ProductsDbContext>(options => options.UseSqlite("Data Source=products.db"));
builder.Services.AddSingleton(cfg);

// logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.FormatterName = ConsoleFormatterNames.Json;
});

//traces
builder.Services.AddOpenTelemetryTracing(options =>
{
    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName))
        .AddEntityFrameworkCoreInstrumentation(opt =>
        {
            opt.SetDbStatementForText = true;
            opt.SetDbStatementForStoredProcedure = true;
        })
        .AddAspNetCoreInstrumentation();
    
    if (!string.IsNullOrWhiteSpace(cfg.ZipkinEndpoint))
    {
        options.AddZipkinExporter(config => config.Endpoint = new Uri(cfg.ZipkinEndpoint));
    }
});

// metrics
builder.Services.AddOpenTelemetryMetrics(options =>
{
    options.ConfigureResource(rb =>
        {
            rb.AddService(ServiceName);
        })
        .AddRuntimeInstrumentation()
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddPrometheusExporter();
});

builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
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

var app = builder.Build();

InitializeDb(app.Services);

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapHealthChecks("/healthz/readiness");
app.MapHealthChecks("/healthz/liveness");

app.Run();

void InitializeDb(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();

    context.Database.EnsureCreated();
    context.Database.Migrate();

    if (!context.Products.Any())
    {
        context.Products.AddRange(new[] {
            new Product(Guid.Parse("b3b749d1-fd02-4b47-8e3c-540555439db6"), "Milk", "Good milk",
                new List<string> { "Food" }, 0.99),
            new Product(Guid.Parse("aaaaaaaa-fd02-4b47-8e3c-540555439db6"), "Coffee", "Delicious Coffee",
                new List<string> { "Food" }, 1.99),
            new Product(Guid.Parse("bbbbbbbb-fd02-4b47-8e3c-540555439db6"), "Coke", "Tasty coke",
                new List<string> { "Food" }, 1.49),
        });
        context.SaveChanges();
    }
}