using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using backend.DataTransferObjects;
using backend.Services;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        //private readonly JwtHelper _jwtHelper;

        public UsersController(UserService userService) //, JwtHelper jwtHelper
        {
            _userService = userService;
            //_jwtHelper = jwtHelper;
        }


        // POST: api/Users/Register
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserDto newUser)
        {
            Console.WriteLine("1 - zoe - Made it");
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Console.WriteLine("2 - zoe - Made it");
            try
            {
                await _userService.RegisterUserAsync(newUser.Username, newUser.Email, newUser.Password);// Create user
                Console.WriteLine("3 - zoe - Made it");
                return Created("api/User/Register", new { message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("4 - zoe - Error");
                return BadRequest(new { error = ex.Message });
            }
        }

        // POST: api/Users/Login
        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserDto loginUser)
        {
            /*
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await GetUserByEmailAsync(loginUser.Email); ; // Get the user

                if (user != null && ValidatePassword(user, loginUser.Password)) // Validate the password
                {
                    var token = GenerateJwtToken(user.Id); // Generate a Jwt Token
                    return Ok(new { access_token = token });
                }

                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            */
            return null;
        }


        // POST: api/Users/Login
        [HttpPost("PAT")]
        [Authorize]
        public async Task<IActionResult> GeneratePat()
        {
            /*
            var userId = User.Identity.Name;
            var tokenGen = TokenGenerator.GenerateToken();
            var hashedToken = TokenHasher.HashToken(tokenGen);

            var token = new PersonalAccessToken
            {
                tokenHash = hashedToken,
                userId = userId,
                CreatedAtAction = DateTime.UtcNow
            };
            */




            return null;
        }

    }
}
