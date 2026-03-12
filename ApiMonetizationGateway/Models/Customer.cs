#nullable disable
namespace ApiMonetizationGateway.Models
{
    public class Customer : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TierId { get; set; }
        public string ApiKey { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Tier Tier { get; set; }
        public ICollection<ApiUsageLog> ApiUsageLogs { get; set; }
        public ICollection<MonthlyUsageSummary> MonthlySummaries { get; set; }
    }
}
