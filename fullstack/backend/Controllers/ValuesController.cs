using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost("receive")]
        public IActionResult ReceivePush([FromBody] PushModel data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            
            if (data.testInt < 0)
            {
                return BadRequest("Id must be a positive number.");
            }

            
            return Ok("Received successfully");
        }

    }
}

public class PushModel
{
    [Required]
    public int testInt { get; set; }

    [MaxLength(3)]
    public string testString { get; set; }
}