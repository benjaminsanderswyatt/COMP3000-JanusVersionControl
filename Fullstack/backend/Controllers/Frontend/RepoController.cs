using backend.Auth;
using backend.DataTransferObjects;
using backend.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace backend.Controllers.Frontend
{
    [Route("api/web/[controller]")]
    [EnableCors("FrontendPolicy")]
    [ApiController]
    [EnableRateLimiting("FrontendRateLimit")]
    public class RepoController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;
        private readonly JwtHelper _jwtHelper;

        public RepoController(JanusDbContext janusDbContext, JwtHelper jwtHelper)
        {
            _janusDbContext = janusDbContext;
            _jwtHelper = jwtHelper;
        }


        // POST: api/web/Users/Register
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto newUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (await _janusDbContext.Users.AnyAsync(u => u.Username == newUser.Username))
                    throw new Exception("Username has already been taken");

                if (await _janusDbContext.Users.AnyAsync(u => u.Email == newUser.Email))
                    throw new Exception("Email has already been taken");

                var (passwordHash, salt) = PasswordSecurity.HashSaltPassword(newUser.Password);

                var user = new User
                {
                    Username = newUser.Username,
                    Email = newUser.Email,
                    PasswordHash = passwordHash,
                    Salt = salt
                };

                _janusDbContext.Users.Add(user);
                await _janusDbContext.SaveChangesAsync();


                return Created("api/web/User/Register", new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        // POST: api/web/Users/Login
        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserDto loginUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Email == loginUser.Email); // Get the user

                if (user == null)
                    return Unauthorized(new { message = "Invalid credentials" });

                if (!PasswordSecurity.VerifyPassword(loginUser.Password, user.PasswordHash, user.Salt))
                    return Unauthorized(new { message = "Invalid credentials" });



                // Generate token
                JwtSecurityToken token = _jwtHelper.GenerateJwtToken(user.UserId, user.Username);

                // Generate refresh token
                var refreshToken = Guid.NewGuid().ToString();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Example: 7 days expiry
                await _janusDbContext.SaveChangesAsync();

                // Set refresh token in HttpOnly cookie
                HttpContext.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = user.RefreshTokenExpiryTime
                });

                return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }




        // POST: api/web/Users/Refresh
        [HttpPost("Refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = HttpContext.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Refresh token is missing." });


            var user = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return Unauthorized(new { message = "Invalid or expired refresh token." });



            // Generate new tokens
            JwtSecurityToken newAccessToken = _jwtHelper.GenerateJwtToken(user.UserId, user.Username);
            var newRefreshToken = Guid.NewGuid().ToString();


            // Update users refresh token in the database
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _janusDbContext.SaveChangesAsync();


            // Set the new refresh token in the HttpOnly cookie
            HttpContext.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = user.RefreshTokenExpiryTime
            });


            return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(newAccessToken) });
        }



        // POST: api/web/Users/Logout
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = HttpContext.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Refresh token is missing." });


            var user = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            // In the case where there is no refresh token found (it doesnt need deleting)
            if (user == null)
                return Ok(new { message = "No refresh token found." });


            // Delete users refresh token in the database
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _janusDbContext.SaveChangesAsync();


            // Expire the refresh token cookie by sending new cookie with an expired date (Jan 1, 1970)
            HttpContext.Response.Cookies.Append("refreshToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = new DateTime(1970, 1, 1)
            });

            return Ok(new { message = "Logged out successfully" });
        }




    }
}
