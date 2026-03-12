using ApiMonetizationGateway.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiMonetizationGateway.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            if (!await context.Tiers.AnyAsync())
            {
                context.Tiers.AddRange(
                    new Tier
                    {
                        Name = "Free",
                        MonthlyQuota = 100,
                        RateLimit = 2,
                        Price = 0.00m,
                        Description = "Free tier with basic limits",
                        CreatedAt = new DateTime(2025, 9, 19)
                    },
                    new Tier
                    {
                        Name = "Pro",
                        MonthlyQuota = 100000,
                        RateLimit = 10,
                        Price = 50.00m,
                        Description = "Professional tier with higher limits",
                        CreatedAt = new DateTime(2025, 9, 19)
                    }
                );
                await context.SaveChangesAsync();
            }

            if (!await context.Customers.AnyAsync())
            {
                var freeTier = await context.Tiers.FirstAsync(t => t.Name == "Free");
                var proTier = await context.Tiers.FirstAsync(t => t.Name == "Pro");

                context.Customers.AddRange(
                    new Customer
                    {
                        Name = "Free Tier Customer",
                        Email = "free@example.com",
                        TierId = freeTier.Id,
                        ApiKey = "free_key",
                        IsActive = true,
                        CreatedAt = new DateTime(2025, 9, 19)
                    },
                    new Customer
                    {
                        Name = "Pro Tier Customer",
                        Email = "pro@example.com",
                        TierId = proTier.Id,
                        ApiKey = "pro_key",
                        IsActive = true,
                        CreatedAt = new DateTime(2025, 9, 19)
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}