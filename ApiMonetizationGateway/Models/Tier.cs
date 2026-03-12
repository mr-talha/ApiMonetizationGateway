#nullable disable
namespace ApiMonetizationGateway.Models
{
    public class Tier : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int MonthlyQuota { get; set; }
        public int RateLimit { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public ICollection<Customer> Customers { get; set; }
    }
}
