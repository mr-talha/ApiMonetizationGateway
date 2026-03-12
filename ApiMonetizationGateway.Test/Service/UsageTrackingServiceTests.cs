using ApiMonetizationGateway.Models;
using ApiMonetizationGateway.Tests.ExtentionServices;
using ApiMonetizationGateway.Services.UsageTrackingService.Impl;

namespace ApiMonetizationGateway.Test.Service
{
    public class UsageTrackingServiceTests
    {
        [Fact]
        public async Task LogUsageAsync_ShouldAddUsageLog()
        {
            var context = TestDbContextFactory.CreateInMemoryDbContext();
            var logger = TestLoggerFactory.CreateLogger<UsageTrackingService>();
            var service = new UsageTrackingService(context, logger);

            await service.LogUsageAsync(1, "/endpoint1", 1, true);

            var logs = context.ApiUsageLogs.ToList();
            Assert.Single(logs);
            Assert.Equal("/endpoint1", logs[0].Endpoint);
        }

        [Fact]
        public async Task ProcessMonthlySummariesAsync_ShouldCreateSummary()
        {
            var context = TestDbContextFactory.CreateInMemoryDbContext();
            var logger = TestLoggerFactory.CreateLogger<UsageTrackingService>();
            var service = new UsageTrackingService(context, logger);

            // Add usage logs
            context.ApiUsageLogs.AddRange(
                new ApiUsageLog { CustomerId = 1, Endpoint = "/endpoint1", Timestamp = DateTime.UtcNow, RequestSize = 5, Success = true },
                new ApiUsageLog { CustomerId = 1, Endpoint = "/endpoint2", Timestamp = DateTime.UtcNow, RequestSize = 10, Success = true }
            );
            await context.SaveChangesAsync();

            await service.ProcessMonthlySummariesAsync();

            var summary = context.MonthlyUsageSummaries.FirstOrDefault();
            Assert.NotNull(summary);
            Assert.Equal(15, summary.TotalRequests);
        }
    }
}