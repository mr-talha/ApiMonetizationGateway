using Microsoft.AspNetCore.Mvc;

namespace ApiMonetizationGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiGatewayController : ControllerBase
    {
        [HttpGet("protected")]
        public IActionResult GetProtectedData()
        {
            return Ok(new
            {
                Message = "Successfully accessed protected endpoint",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
