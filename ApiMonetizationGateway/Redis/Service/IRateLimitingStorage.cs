namespace ApiMonetizationGateway.Redis.Service
{
    public interface IRateLimitingStorage
    {
        Task<long> StringIncrementAsync(string key);
        Task<long> StringDecrementAsync(string key);
        Task<List<long>> ListRangeAsync(string key);
        Task ListLeftPushAsync(string key, long value);
        Task ListTrimAsync(string key, int start, int end);
    }
}
