using backend.DataTransferObjects;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    public class TokensController : Controller
    {
        [Route("api/[controller]")]
        [ApiController]
        public class UsersController : ControllerBase
        {
            // POST: api/Users/Token/Login
            [HttpPost("Token/Login")]
            public async Task<IActionResult> TokenLogin([FromBody] RegisterUserDto newUser)
            {
                return null;
            }
        }
    }
}
