using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace ApiMonetizationGateway.Redis
{
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly IConnectionMultiplexer _redis;

        public RedisHealthCheck(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if Redis is connected and responsive
                var db = _redis.GetDatabase();
                var pingResult = await db.PingAsync();

                return pingResult > TimeSpan.Zero
                    ? HealthCheckResult.Healthy("Redis is healthy")
                    : HealthCheckResult.Unhealthy("Redis ping failed");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Redis health check failed", ex);
            }
        }
    }
}