using ApiMonetizationGateway.Tests.ExtentionServices;
using ApiMonetizationGateway.Services.CustomerService.Impl;

namespace ApiMonetizationGateway.Test.Service
{
    public class CustomerServiceTests
    {
        [Fact]
        public async Task GetCustomerByApiKeyAsync_ReturnsCustomer()
        {
            var context = TestDbContextFactory.CreateInMemoryDbContext();
            var service = new CustomerService(context);

            var customer = await service.GetCustomerByApiKeyAsync("key123");

            Assert.NotNull(customer);
            Assert.Equal("Test", customer.Name);
        }

        [Fact]
        public async Task GetTiersAsync_ReturnsAllTiers()
        {
            var context = TestDbContextFactory.CreateInMemoryDbContext();
            var service = new CustomerService(context);

            var tiers = await service.GetTiersAsync();

            Assert.Single(tiers);
            Assert.Equal("Free", tiers[0].Name);
        }
    }
}