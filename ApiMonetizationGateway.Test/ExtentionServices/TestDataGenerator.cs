using ApiMonetizationGateway.Data;
using ApiMonetizationGateway.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiMonetizationGateway.Test.ExtentionServices
{
    public static class TestDataGenerator
    {
        public static Tier CreateFreeTier() => new()
        {
            Id = 1,
            Name = "Free",
            MonthlyQuota = 100,
            RateLimit = 2,
            Price = 0.00m,
            Description = "Free tier"
        };

        public static Tier CreateProTier() => new()
        {
            Id = 2,
            Name = "Pro",
            MonthlyQuota = 100000,
            RateLimit = 10,
            Price = 50.00m,
            Description = "Pro tier"
        };

        public static Customer CreateFreeCustomer() => new()
        {
            Id = 1,
            Name = "Free Customer",
            Email = "free@test.com",
            TierId = 1,
            ApiKey = "free_key",
            IsActive = true,
            Tier = CreateFreeTier()
        };

        public static Customer CreateProCustomer() => new()
        {
            Id = 2,
            Name = "Pro Customer",
            Email = "pro@test.com",
            TierId = 2,
            ApiKey = "pro_key",
            IsActive = true,
            Tier = CreateProTier()
        };

        public static ApiUsageLog CreateApiUsageLog(int customerId = 1, bool success = true) => new()
        {
            CustomerId = customerId,
            Endpoint = "/api/test",
            Timestamp = DateTime.UtcNow,
            RequestSize = 1,
            Success = success
        };

        public static ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            // Seed data
            var tier = new Tier { Id = 1, Name = "Free", MonthlyQuota = 100, RateLimit = 2, Price = 0 };
            context.Tiers.Add(tier);
            var customer = new Customer { Id = 1, Name = "Test Customer", Email = "test@example.com", ApiKey = "test_key", TierId = 1, IsActive = true };
            context.Customers.Add(customer);

            context.SaveChanges();
            return context;
        }
    }
}