using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CLIPolicy")]
    public class TestController : ControllerBase
    {



        // POST: api/Test/SayHello
        [HttpPost("SayHello")]
        public async Task<IActionResult> SayHello([FromBody] string test)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                Console.WriteLine($"Test printed: {test}");

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


    }
}
