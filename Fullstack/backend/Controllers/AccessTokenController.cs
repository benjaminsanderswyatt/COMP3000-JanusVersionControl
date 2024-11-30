using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Authorize]
    [Route("api/CLI/[controller]")]
    [ApiController]
    public class AccessTokenController : ControllerBase
    {
        private readonly AccessTokenHelper _accessTokenHelper;

        public AccessTokenController(AccessTokenHelper accessTokenHelper)
        {
            _accessTokenHelper = accessTokenHelper;
        }

        // POST: api/CLI/AccessToken/GenPAT
        [HttpPost("GenPAT")]
        public async Task<IActionResult> GeneratePat()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID not found in the token.");
            }


            var token = await _accessTokenHelper.GenerateTokenAsync(userId);

            return Ok(token);
        }

        // POST: api/CLI/AccessToken/Validate
        [HttpPost("Validate")]
        public async Task<IActionResult> ValidateToken([FromBody] string token)
        {
            var isValid = await _accessTokenHelper.ValidateTokenAsync(token);

            if (isValid)
            {
                return Ok(true);  // Token is valid
            }
            else
            {
                return Ok(false); // Token is invalid or expired
            }
        }


    }
}
