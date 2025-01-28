using backend.DataTransferObjects;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("FrontendRateLimit")]
    public class AccessTokenController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;
        private readonly AccessTokenHelper _accessTokenHelper;

        public AccessTokenController(JanusDbContext janusDbContext, AccessTokenHelper accessTokenHelper)
        {
            _janusDbContext = janusDbContext;
            _accessTokenHelper = accessTokenHelper;
        }

        // POST: api/AccessToken/GeneratePAT
        [Authorize(Policy = "FrontendPolicy")]
        [EnableCors("FrontendPolicy")]
        [HttpPost("GeneratePAT")]
        public async Task<IActionResult> GeneratePAT([FromBody] PatDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            Console.WriteLine($"jepted Entered");


            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "User not found" });
            }

            // Validate expiration time (min 12 hours, max 1 year)
            if (request.ExpirationInHours < 12 || request.ExpirationInHours > 8760) // 8760 hours = 1 year
            {
                return BadRequest(new { message = "Expiration time must be between 12 hours and 1 year." });
            }

            Console.WriteLine($"Request: {request.ExpirationInHours}");


            var token = _accessTokenHelper.GenerateAccessToken(userId, request.ExpirationInHours);

            Console.WriteLine($"jepted Token: {token}");

            return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token)});
        }



        // POST: api/AccessToken/RevokePAT
        [Authorize(Policy = "CLIPolicy")]
        [EnableCors("CLIPolicy")]
        [HttpPost("RevokePAT")]
        public async Task<IActionResult> RevokePAT(string patToken)
        {
            // Check if the token is already blacklisted
            var existingToken = await _janusDbContext.AccessTokenBlacklists
                .FirstOrDefaultAsync(t => t.Token == patToken);

            if (existingToken != null)
            {
                return BadRequest(new { message = "This token is already revoked." });
            }



            // Get the exp claim from the JWT
            var exp = User.FindFirst("exp")?.Value;

            if (string.IsNullOrEmpty(exp) || !long.TryParse(exp, out var expUnix))
            {
                return BadRequest(new { error = "Invalid or missing expiration date." });
            }

            // Convert the expiration time (Unix timestamp) to a DateTime
            var expirationDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;

            // Add token to blacklist
            var token = new AccessTokenBlacklist
            {
                Token = patToken,
                Expires = expirationDate
            };

            _janusDbContext.AccessTokenBlacklists.Add(token);
            await _janusDbContext.SaveChangesAsync();

            return Ok(new { message = "Token has been revoked." });
        }

    }
}
