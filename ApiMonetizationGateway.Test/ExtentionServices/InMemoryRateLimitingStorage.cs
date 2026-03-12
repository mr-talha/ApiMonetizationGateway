using System.Collections.Concurrent;
using ApiMonetizationGateway.Redis.Service;

namespace ApiMonetizationGateway.Tests.ExtentionServices
{
    public class InMemoryRateLimitingStorage : IRateLimitingStorage
    {
        private readonly ConcurrentDictionary<string, List<long>> _listStorage = new();
        private readonly ConcurrentDictionary<string, long> _stringStorage = new();

        public Task<long> StringIncrementAsync(string key)
        {
            var newValue = _stringStorage.AddOrUpdate(key, 1, (_, old) => old + 1);
            return Task.FromResult(newValue);
        }

        public Task<long> StringDecrementAsync(string key)
        {
            var newValue = _stringStorage.AddOrUpdate(key, 0, (_, old) => old > 0 ? old - 1 : 0);
            return Task.FromResult(newValue);
        }

        public Task<List<long>> ListRangeAsync(string key)
        {
            _listStorage.TryGetValue(key, out var list);
            return Task.FromResult(list ?? new List<long>());
        }

        public Task ListLeftPushAsync(string key, long value)
        {
            _listStorage.AddOrUpdate(key,
                k => new List<long> { value },
                (_, old) => { old.Add(value); return old; });
            return Task.CompletedTask;
        }

        public Task ListTrimAsync(string key, int start, int end)
        {
            if (_listStorage.TryGetValue(key, out var list))
            {
                var trimmed = list.Skip(start).Take(end - start + 1).ToList();
                _listStorage[key] = trimmed;
            }
            return Task.CompletedTask;
        }
    }
}
