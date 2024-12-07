using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.Controllers
{
    
    [Route("api/CLI/[controller]")]
    [ApiController]
    public class AccessTokenController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;
        private readonly AccessTokenHelper _accessTokenHelper;

        public AccessTokenController(JanusDbContext janusDbContext, AccessTokenHelper accessTokenHelper)
        {
            _janusDbContext = janusDbContext;
            _accessTokenHelper = accessTokenHelper;
        }

        // POST: api/web/Users/GenPAT
        [Authorize(Policy = "FrontendPolicy")]
        [EnableCors("FrontendPolicy")]
        [HttpPost("GenPAT")]
        public async Task<IActionResult> GeneratePAT()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("User ID not found in the token.");
            }


            var token = _accessTokenHelper.GenerateAccessToken(userId);

            return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
        }


        // POST: api/web/Users/GenPAT
        [Authorize(Policy = "CLIPolicy")]
        [EnableCors("CLIPolicy")]
        [HttpPost("RevokePAT")]
        public async Task<IActionResult> RevokePAT(string patToken)
        {
            var tokenHash = _accessTokenHelper.HashToken(patToken);

            // Check if the token is already blacklisted
            var existingToken = await _janusDbContext.AccessTokenBlacklists
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (existingToken != null)
            {
                return BadRequest("This token is already revoked.");
            }

            // Add token to blacklist
            var token = new AccessTokenBlacklist
            {
                TokenHash = tokenHash,
                Expires = DateTime.UtcNow.AddMonths(3)
            };

            _janusDbContext.AccessTokenBlacklists.Add(token);
            await _janusDbContext.SaveChangesAsync();
            
            return Ok("Token has been revoked.");
        }

    }
}
