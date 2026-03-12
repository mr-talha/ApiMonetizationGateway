namespace ApiMonetizationGateway.Services.UsageTrackingService
{
    public interface IUsageTrackingService
    {
        Task LogUsageAsync(int customerId, string endpoint, int requestSize = 1, bool success = true, string? userId = null);
        Task ProcessMonthlySummariesAsync();
    }
}
