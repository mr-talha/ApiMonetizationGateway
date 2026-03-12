using ApiMonetizationGateway.Data;
using ApiMonetizationGateway.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiMonetizationGateway.Tests.ExtentionServices
{
    public static class TestDbContextFactory
    {
        public static ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // unique DB for each call
                .Options;

            var context = new ApplicationDbContext(options);

            // Seed Tier
            context.Tiers.Add(new Tier
            {
                Id = 1,
                Name = "Free",
                MonthlyQuota = 100,
                RateLimit = 2,
                Price = 0
            });

            // Seed Customer
            context.Customers.Add(new Customer
            {
                Id = 1,
                Name = "Test",
                Email = "test@example.com",
                ApiKey = "key123",
                TierId = 1,
                IsActive = true
            });

            context.SaveChanges();

            return context;
        }
    }
}
