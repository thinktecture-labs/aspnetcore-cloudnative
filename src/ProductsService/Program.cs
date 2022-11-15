using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using ProductsService.Configuration;
using ProductsService.Data;
using ProductsService.Data.Entities;
using ProductsService.Json;
using ProductsService.Services;

var builder = WebApplication.CreateBuilder(args);

var configurationPath = builder.Configuration["ConfigurationPath"];
if (!String.IsNullOrWhiteSpace(configurationPath))
{
    builder.Configuration.AddKeyPerFile(configurationPath, true, true);
}

var config = new ProductsServiceConfiguration();
var cfgSection = builder.Configuration.GetSection(ProductsServiceConfiguration.SectionName);
cfgSection.Bind(config);

builder.Logging.ConfigureLogging();

builder.Services.AddHttpClient<CurrencyService>(client =>
{
    client.BaseAddress = new Uri(config.ExchangeRateUrl);
});

#region Stuff
    // .AddPolicyHandler(Policy
    //     .HandleResult<HttpResponseMessage>(m => !m.IsSuccessStatusCode)
    //     .Or<HttpRequestException>()
    //     .Or<TimeoutRejectedException>()
    //     .FallbackAsync(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
    //     {
    //         Content = new ByteArrayContent(File.ReadAllBytes("Data/fallbackExchangeRates.json"))
    //     }))
    // ).AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(2)));
#endregion

builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNameCaseInsensitive = true;
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    options.Converters.Add(new DateOnlyConverter());
});

builder.Services.AddOpenTelemetry(config);
builder.Services.AddHealthChecks();
builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new DateOnlyConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();
builder.Services.AddDbContext<ProductsDbContext>(options => options.UseSqlite("Data Source=products.db"));

var app = builder.Build();
InitializeDb(app.Services);

if (!String.IsNullOrEmpty(config.PathBase))
{
    app.UsePathBase(config.PathBase);
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