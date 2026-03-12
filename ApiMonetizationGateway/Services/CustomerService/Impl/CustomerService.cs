using ApiMonetizationGateway.Data;
using ApiMonetizationGateway.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiMonetizationGateway.Services.CustomerService.Impl
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Customer?> GetCustomerByApiKeyAsync(string apiKey)
        {
            return await _context.Customers
                .Include(c => c.Tier)
                .FirstOrDefaultAsync(c => c.ApiKey == apiKey && c.IsActive);
        }

        public async Task<List<Tier>> GetTiersAsync()
        {
            return await _context.Tiers.ToListAsync();
        }
    }
}
