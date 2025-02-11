using backend.DataTransferObjects;
using backend.Models;
using backend.Services;
using backend.Utils.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace backend.Controllers.Frontend
{
    [Route("api/web/[controller]")]
    [EnableCors("FrontendPolicy")]
    [ApiController]
    [Authorize(Policy = "FrontendPolicy")]
    [EnableRateLimiting("FrontendRateLimit")]
    public class AccountController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;
        private readonly JwtHelper _jwtHelper;
        private readonly ProfilePicManagement _profilePicManagement;

        public AccountController(JanusDbContext janusDbContext, JwtHelper jwtHelper, ProfilePicManagement profilePicManagement)
        {
            _janusDbContext = janusDbContext;
            _jwtHelper = jwtHelper;
            _profilePicManagement = profilePicManagement;
        }


        [HttpGet("GetProfilePic")]
        public async Task<IActionResult> GetProfilePicture()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            var filePath = await _profilePicManagement.GetProfilePicturePathAsync(userId);

            if (filePath == null)
                return NotFound();


            if (!System.IO.File.Exists(filePath))
                return NotFound();


            return PhysicalFile(filePath, "image/jpeg");
        }



        // Save a (new) profile picture
        // POST: api/web/Account/ChangeProfilePicture
        [HttpPost("ChangeProfilePicture")]
        public async Task<IActionResult> ChangeProfilePicture(IFormFile image)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            if (image == null || image.Length == 0)
            {
                return BadRequest("Invalid file");
            }


            string uploadsFolder = Path.Combine("/app/images", userId.ToString());

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileExtension = Path.GetExtension(image.FileName);
            string fileName = $"profile{fileExtension}";
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            ReturnObject result = await _profilePicManagement.SaveProfilePicturePathAsync(userId, filePath);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }






    }
}
