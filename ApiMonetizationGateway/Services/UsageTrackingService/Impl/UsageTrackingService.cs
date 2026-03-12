using ApiMonetizationGateway.Data;
using ApiMonetizationGateway.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiMonetizationGateway.Services.UsageTrackingService.Impl
{
    public class UsageTrackingService : IUsageTrackingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsageTrackingService> _logger;

        public UsageTrackingService(ApplicationDbContext context, ILogger<UsageTrackingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogUsageAsync(int customerId, string endpoint, int requestSize = 1, bool success = true, string? userId = null)
        {
            try
            {
                var usageLog = new ApiUsageLog
                {
                    CustomerId = customerId,
                    UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
                    Endpoint = endpoint,
                    Timestamp = DateTime.UtcNow,
                    RequestSize = requestSize,
                    Success = success
                };

                await _context.ApiUsageLogs.AddAsync(usageLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging API usage for customer {CustomerId}", customerId);
            }
        }

        public async Task ProcessMonthlySummariesAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var monthYear = now.ToString("yyyy-MM");
                var month = now.Month;
                var year = now.Year;

                var usageData = await _context.ApiUsageLogs
                    .Where(u => u.Timestamp.Month == month && u.Timestamp.Year == year && u.Success)
                    .Include(u => u.Customer)
                        .ThenInclude(c => c.Tier)
                    .GroupBy(u => u.CustomerId)
                    .Select(g => new
                    {
                        CustomerId = g.Key,
                        TotalRequests = g.Sum(u => u.RequestSize),
                        Customer = g.First().Customer
                    })
                    .ToListAsync();

                foreach (var data in usageData)
                {
                    var totalCost = CalculateCost(data.Customer.Tier, data.TotalRequests);

                    var summary = await _context.MonthlyUsageSummaries
                        .FirstOrDefaultAsync(s => s.CustomerId == data.CustomerId && s.MonthYear == monthYear);

                    if (summary == null)
                    {
                        summary = new MonthlyUsageSummary
                        {
                            CustomerId = data.CustomerId,
                            MonthYear = monthYear,
                            TierId = data.Customer.TierId
                        };
                        await _context.MonthlyUsageSummaries.AddAsync(summary);
                    }

                    summary.TotalRequests = data.TotalRequests;
                    summary.TotalCost = totalCost;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Monthly summaries processed successfully for {MonthYear}", monthYear);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing monthly summaries");
            }
        }

        private decimal CalculateCost(Tier tier, int totalRequests)
        {
            if (tier.Price == 0) return 0;

            if (totalRequests <= tier.MonthlyQuota)
                return tier.Price;

            // Charge $0.01 per extra request
            var overage = totalRequests - tier.MonthlyQuota;
            return tier.Price + (overage * 0.01m);
        }
    }
}