#nullable disable
namespace ApiMonetizationGateway.Models
{
    public class MonthlyUsageSummary : BaseEntity
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string MonthYear { get; set; }
        public int TotalRequests { get; set; }
        public decimal TotalCost { get; set; }
        public int TierId { get; set; }
        public Customer Customer { get; set; }
        public Tier Tier { get; set; }
    }
}
