using ApiMonetizationGateway.Models;

namespace ApiMonetizationGateway.Services.CustomerService
{
    public interface ICustomerService
    {
        Task<Customer?> GetCustomerByApiKeyAsync(string apiKey);
        Task<List<Tier>> GetTiersAsync();
    }
}
