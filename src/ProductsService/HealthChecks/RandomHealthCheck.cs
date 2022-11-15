using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ProductsService.HealthChecks;

public class RandomHealthCheck: IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken()) => 
        Task.FromResult(Random.Shared.Next(Int32.MaxValue) % 2 == 0 ? HealthCheckResult.Degraded("Random sucks") : HealthCheckResult.Healthy() );
}