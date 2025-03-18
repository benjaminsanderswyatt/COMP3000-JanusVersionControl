using backend.DataTransferObjects;
using backend.DataTransferObjects.CLI;
using backend.Models;
using backend.Utils.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace backend.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AccessTokenController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;
        private readonly AccessTokenHelper _accessTokenHelper;
        private readonly UserManagement _userManagement;

        public AccessTokenController(JanusDbContext janusDbContext, AccessTokenHelper accessTokenHelper, UserManagement userManagement)
        {
            _janusDbContext = janusDbContext;
            _accessTokenHelper = accessTokenHelper;
            _userManagement = userManagement;
        }

        // POST: api/AccessToken/GeneratePAT
        [EnableRateLimiting("FrontendRateLimit")]
        [Authorize(Policy = "FrontendPolicy")]
        [EnableCors("FrontendPolicy")]
        [HttpPost("GeneratePAT")]
        public async Task<IActionResult> GeneratePAT([FromBody] PatDto request)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;


            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "User not found" });
            }

            // Validate expiration time (min 12 hours, max 1 year)
            if (request.ExpirationInHours < 12 || request.ExpirationInHours > 8760) // 8760 hours = 1 year
            {
                return BadRequest(new { message = "Expiration time must be between 12 hours and 1 year" });
            }



            var token = _accessTokenHelper.GenerateAccessToken(userId, request.ExpirationInHours);

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }



        // POST: api/AccessToken/RevokePAT
        [EnableRateLimiting("CLIRateLimit")]
        [Authorize(Policy = "CLIPolicy")]
        [EnableCors("CLIPolicy")]
        [HttpPost("RevokePAT")]
        public async Task<IActionResult> RevokePAT([FromHeader(Name = "Authorization")] string authHeader)
        {
            // Remove the Bearer
            string patToken = authHeader.Substring("Bearer ".Length).Trim();


            // Check if the token is already blacklisted
            var existingToken = await _janusDbContext.AccessTokenBlacklists
                .FirstOrDefaultAsync(t => t.Token == patToken);

            if (existingToken != null)
            {
                return BadRequest(new { message = "This token is already revoked" });
            }


            // Get the exp claim from the JWT
            var exp = User.FindFirst("exp")?.Value;

            if (string.IsNullOrEmpty(exp) || !long.TryParse(exp, out var expUnix))
            {
                return BadRequest(new { error = "Invalid or missing expiration date" });
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

            return Ok(new { message = "Token has been revoked" });
        }




        public class RevokePatDto
        {
            public string PatToken { get; set; }
        }


        // POST: api/AccessToken/RevokePATFrontend
        [EnableRateLimiting("FrontendRateLimit")]
        [Authorize(Policy = "FrontendPolicy")]
        [EnableCors("FrontendPolicy")]
        [HttpPost("RevokePATFrontend")]
        public async Task<IActionResult> RevokePATFrontend([FromBody] RevokePatDto PAT)
        {
            var accessTokenHelper = new AccessTokenHelper();

            SecurityToken validatedToken;

            try
            {
                var principal = accessTokenHelper.ValidateAccessToken(PAT.PatToken, out validatedToken);
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Invalid PAT" });
            }




            var jwtToken = validatedToken as JwtSecurityToken;
            if (jwtToken == null)
            {
                return BadRequest(new { message = "Invalid PAT" });
            }



            // Check the token type claim
            var tokenTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "TokenType")?.Value;
            if (string.IsNullOrEmpty(tokenTypeClaim) || tokenTypeClaim != "PAT")
            {
                return BadRequest(new { message = "Invalid PAT" });
            }


            // Get the expiration claim from the PAT
            var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
            if (string.IsNullOrEmpty(expClaim) || !long.TryParse(expClaim, out var expUnix))
            {
                return BadRequest(new { message = "Invalid PAT" });
            }

            var expirationDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;

            if (expirationDate < DateTime.UtcNow)
            {
                return BadRequest(new { message = "PAT has already expired" });
            }



            // Check if the token is already blacklisted
            var existingToken = await _janusDbContext.AccessTokenBlacklists
                .FirstOrDefaultAsync(t => t.Token == PAT.PatToken);

            if (existingToken != null)
            {
                return BadRequest(new { message = "This PAT is already revoked" });
            }




            // Add token to blacklist
            var token = new AccessTokenBlacklist
            {
                Token = PAT.PatToken,
                Expires = expirationDate
            };

            _janusDbContext.AccessTokenBlacklists.Add(token);
            await _janusDbContext.SaveChangesAsync();

            return Ok(new { message = "Token has been revoked" });
        }






        // POST: api/AccessToken/Authenticate
        [EnableRateLimiting("CLIRateLimit")]
        [Authorize(Policy = "CLIPolicy")]
        [EnableCors("CLIPolicy")]
        [HttpPost("Authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthPatDto auth)
        {
            Console.WriteLine($"Entered: {auth.Email}");


            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Message = "Invalid user" });
            }

            Console.WriteLine($"id: {userId}");

            var user = await _userManagement.GetUserByIdAsync(userId);

            if (user == null)
                return Unauthorized(new { Message = "Invalid credentials" });


            if (user.Email != auth.Email)
                return Unauthorized(new { Message = "Invalid credentials" });


            Console.WriteLine($"Success");
            return Ok(new { Username = user.Username });
        }


    }
}
