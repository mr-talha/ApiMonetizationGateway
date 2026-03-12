using ApiMonetizationGateway.Data;
using ApiMonetizationGateway.Models;
using Microsoft.EntityFrameworkCore;
using ApiMonetizationGateway.Services.CustomerService;
using ApiMonetizationGateway.Services.CustomerService.Impl;
using ApiMonetizationGateway.Services.RateLimitingService.Impl;
using ApiMonetizationGateway.Tests.ExtentionServices;

namespace ApiMonetizationGateway.Tests.Service
{
    public class RateLimitingServiceTests
    {
        private ApplicationDbContext GetContext(int monthlyQuota = 5, int rateLimit = 2)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            context.Tiers.Add(new Tier { Id = 1, Name = "Free", MonthlyQuota = monthlyQuota, RateLimit = rateLimit, Price = 0 });
            context.Customers.Add(new Customer
            {
                Id = 1,
                Name = "Test Customer",
                Email = "test@example.com",
                ApiKey = "key123",
                TierId = 1,
                IsActive = true
            });
            context.SaveChanges();
            return context;
        }

        private ICustomerService GetCustomerService(ApplicationDbContext context)
        {
            return new CustomerService(context);
        }

        [Fact]
        public async Task CheckRateLimit_AllowsWithinLimit()
        {
            var context = GetContext();
            var customerService = GetCustomerService(context);
            var storage = new InMemoryRateLimitingStorage();
            var service = new RateLimitingService(storage, customerService);

            var result = await service.CheckRateLimitAsync("key123", "/endpoint1");

            Assert.True(result.IsAllowed);
            Assert.Equal("OK", result.Message);
        }

        [Fact]
        public async Task CheckRateLimit_BlocksRateLimitExceeded()
        {
            var context = GetContext();
            var customerService = GetCustomerService(context);
            var storage = new InMemoryRateLimitingStorage();
            var service = new RateLimitingService(storage, customerService);

            // Free tier rate limit = 2 per second
            await service.CheckRateLimitAsync("key123", "/endpoint1");
            await service.CheckRateLimitAsync("key123", "/endpoint1");

            var result = await service.CheckRateLimitAsync("key123", "/endpoint1");

            Assert.False(result.IsAllowed);
            Assert.Equal("Rate limit exceeded", result.Message);
        }

        [Fact]
        public async Task CheckRateLimit_BlocksMonthlyQuotaExceeded()
        {
            var context = GetContext(monthlyQuota: 5, rateLimit: 1000);
            var customerService = GetCustomerService(context);
            var storage = new InMemoryRateLimitingStorage();
            var service = new RateLimitingService(storage, customerService);

            // Free tier monthly quota = 5
            for (int i = 0; i < 5; i++)
            {
                await service.CheckRateLimitAsync("key123", $"/endpoint{i}");
            }

            var result = await service.CheckRateLimitAsync("key123", "/endpoint-extra");

            Assert.False(result.IsAllowed);
            Assert.Equal("Monthly quota exceeded", result.Message);
        }
    }
}
