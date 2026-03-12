using ApiMonetizationGateway.Services.UsageTrackingService;

namespace ApiMonetizationGateway.BackgroundServices
{
    public class MonthlySummaryService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MonthlySummaryService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(24); // Run daily
        //private readonly TimeSpan _interval = TimeSpan.FromMinutes(1); // every minute

        public MonthlySummaryService(IServiceProvider serviceProvider, ILogger<MonthlySummaryService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var usageService = scope.ServiceProvider.GetRequiredService<IUsageTrackingService>();

                    await usageService.ProcessMonthlySummariesAsync();
                    _logger.LogInformation("Monthly summaries processed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing monthly summaries");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}