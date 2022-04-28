using Dapr.Client;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrdersService.Configuration;
using OrdersService.Repositories;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);
var cfg = new OrdersServiceConfiguration();
var cfgSection = builder.Configuration.GetSection(OrdersServiceConfiguration.SectionName);
if (cfgSection == null || !cfgSection.Exists())
{
    throw new ApplicationException(
        $"Could not find service config. Please provide a '{OrdersServiceConfiguration.SectionName}' config section");
}
else
{
    cfgSection.Bind(cfg);
}
builder.Services.AddSingleton(cfg);
builder.Services.AddScoped<IOrdersRepository, FakeOrdersRepository>();
builder.Services.AddScoped<DaprClient>(_ => new DaprClientBuilder().Build()!);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Orders Service",
        Description = "Fairly simple .NET API to interact with orders",
        Contact = new OpenApiContact
        {
            Name = "Thinktecture AG",
            Email = "info@thinktecture.com",
            Url = new Uri("https://thinktecture.com")
        }
    });
});
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();
app.MapMetrics();
app.UseHttpMetrics();
app.MapHealthChecks("/healthz/readiness");
app.MapHealthChecks("/healthz/liveness");
app.Run();
