using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiMonetizationGateway.Tests.Integration
{
    public class ApiGatewayIntegrationTests 
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ApiGatewayIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task ProtectedEndpoint_WithValidApiKey_ReturnsOK()
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "/api/ApiGateway/protected"
            );

            request.Headers.Add("X-API-Key", "free_key");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ProtectedEndpoint_WithInvalidApiKey_ReturnsUnauthorized()
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "/api/ApiGateway/protected"
            );

            request.Headers.Add("X-API-Key", "invalid_key");

            var response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RateLimitExceeded_Returns429()
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "/api/ApiGateway/protected"
            );

            request.Headers.Add("X-API-Key", "free_key");

            await _client.SendAsync(request);
            await _client.SendAsync(request);

            var response = await _client.SendAsync(request);

            Assert.Equal((HttpStatusCode)429, response.StatusCode);
        }
    }
}