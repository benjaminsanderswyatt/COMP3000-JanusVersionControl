using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        // Handle OPTIONS requests explicitly (for testing)
        [HttpOptions("options")]
        public IActionResult HandleOptions()
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:81");
            Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept");
            return Ok();
        }

        // Any other test endpoint you want to test with
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "CORS is working" });
        }
    }
}