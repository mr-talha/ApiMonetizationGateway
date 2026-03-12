#nullable disable
namespace ApiMonetizationGateway.Models
{
    public class ApiUsageLog : BaseEntity
    {
        public long Id { get; set; }
        public int CustomerId { get; set; }
        public string UserId { get; set; }
        public string Endpoint { get; set; }
        public DateTime Timestamp { get; set; }
        public int RequestSize { get; set; } = 1;
        public bool Success { get; set; } = true;
        public Customer Customer { get; set; }
    }
}
