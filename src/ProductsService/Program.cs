using Microsoft.EntityFrameworkCore;
using ProductsService.Configuration;
using ProductsService.Data;
using ProductsService.Data.Entities;

var builder = WebApplication.CreateBuilder(args);

var cfg = new ProductsServiceConfiguration();
var cfgSection = builder.Configuration.GetSection(ProductsServiceConfiguration.SectionName);

if (cfgSection == null || !cfgSection.Exists())
{
    throw new ApplicationException(
        $"Could not find service config. Please provide a '{ProductsServiceConfiguration.SectionName}' config section");
}
cfgSection.Bind(cfg);

builder.Logging.ConfigureLogging();

builder.Services.AddOpenTelemetry(cfg);
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();
builder.Services.AddDbContext<ProductsDbContext>(options => options.UseSqlite("Data Source=products.db"));


var app = builder.Build();

InitializeDb(app.Services);

if (!String.IsNullOrEmpty(cfg.PathBase))
{
    app.UsePathBase(cfg.PathBase);
    app.UseRouting();
}

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