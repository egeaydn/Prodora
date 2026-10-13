using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prodora.DataAccess.Concrate.EfCore;
using Prodora.WebUI.Identity;

namespace Prodora.WebUI.Health;

/// <summary>Readiness: the commerce and identity databases both accept connections. The response never carries details.</summary>
public sealed class DatabaseHealthCheck(IDbContextFactory<DataContext> commerce, IServiceScopeFactory scopes, ILogger<DatabaseHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var commerceContext = await commerce.CreateDbContextAsync(cancellationToken);
            using var scope = scopes.CreateScope();
            var identity = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();
            var reachable = await commerceContext.Database.CanConnectAsync(cancellationToken)
                && await identity.Database.CanConnectAsync(cancellationToken);
            return reachable ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("database unreachable");
        }
        catch (Exception exception)
        {
            logger.LogWarning("Readiness check failed; error type {ErrorType}", exception.GetType().Name);
            return HealthCheckResult.Unhealthy("database check failed");
        }
    }
}
