using backend.Auth;
using backend.DataTransferObjects;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/web/[controller]")]
    [EnableCors("FrontendPolicy")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;
        private readonly JwtHelper _jwtHelper;

        public UsersController(JanusDbContext janusDbContext, JwtHelper jwtHelper)
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
                return BadRequest(new { error = ex.Message });
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

                if (user != null && PasswordSecurity.VerifyPassword(loginUser.Password, user.PasswordHash, user.Salt)) // Validate the password
                {
                    JwtSecurityToken token = _jwtHelper.GenerateJwtToken(user.UserId, user.Username); // Generate a Jwt Token

                    return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
                }

                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }



    }
}
