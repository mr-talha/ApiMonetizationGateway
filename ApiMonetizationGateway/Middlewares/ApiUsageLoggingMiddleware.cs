using ApiMonetizationGateway.Data;
using ApiMonetizationGateway.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

#nullable disable
namespace ApiMonetizationGateway.Middlewares
{
    public class ApiUsageLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiUsageLoggingMiddleware> _logger;

        public ApiUsageLoggingMiddleware(RequestDelegate next, ILogger<ApiUsageLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            try
            {
                // Extract API key from headers (X-API-Key)
                var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();

                Customer customer = null;

                if (!string.IsNullOrEmpty(apiKey))
                {
                    customer = await db.Customers
                        .FirstOrDefaultAsync(c => c.ApiKey == apiKey && c.IsActive);
                }

                // Call the next middleware
                await _next(context);

                // Only log if a valid customer is found
                if (customer != null)
                {
                    var userId =
                        context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        context.User?.FindFirstValue("sub") ??
                        context.Request.Headers["X-User-Id"].FirstOrDefault();

                    var log = new ApiUsageLog
                    {
                        CustomerId = customer.Id,
                        UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
                        Endpoint = context.Request.Path,
                        Timestamp = DateTime.UtcNow,
                        Success = context.Response.StatusCode < 400
                    };

                    db.ApiUsageLogs.Add(log);
                    await db.SaveChangesAsync();
                    _logger.LogInformation($"API usage logged for Customer {customer.Id} - {context.Request.Path}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log API usage");
            }
        }
    }
}