namespace ApiMonetizationGateway.Services.RateLimitingService
{
    public interface IRateLimitingService
    {
        Task<(bool IsAllowed, string Message)> CheckRateLimitAsync(string apiKey, string endpoint);
    }
}
