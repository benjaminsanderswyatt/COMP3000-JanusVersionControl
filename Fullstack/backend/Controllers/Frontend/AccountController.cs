using backend.Models;
using backend.Utils.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

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

        private readonly string _imagePath = Path.Combine(Directory.GetCurrentDirectory(), "/app/data/images");

        public AccountController(JanusDbContext janusDbContext, JwtHelper jwtHelper, ProfilePicManagement profilePicManagement)
        {
            _janusDbContext = janusDbContext;
            _jwtHelper = jwtHelper;
            _profilePicManagement = profilePicManagement;
        }


        // Save a (new) profile picture
        // POST: api/web/Account/ChangeProfilePicture
        [HttpPost("ChangeProfilePicture")]
        public async Task<IActionResult> ChangeProfilePicture(IFormFile image)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            if (image == null || image.Length == 0)
            {
                return BadRequest(new { message = "Invalid file" });
            }


            string fileExtension = Path.GetExtension(image.FileName);
            string fileName = $"{userId}{fileExtension}";
            string filePath = Path.Combine(_imagePath, fileName);


            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            ReturnObject result = await _profilePicManagement.SaveProfilePicturePathAsync(userId, filePath);


            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "Profile picture updated successfully", profilePictureUrl = $"/images/{fileName}" });
        }




        // DELETE: api/web/Account/Delete
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteUser()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Invalid user ID" });
            }


            string fileDir = Environment.GetEnvironmentVariable("FILE_STORAGE_PATH");
            string treeDir = Environment.GetEnvironmentVariable("TREE_STORAGE_PATH");


            var user = await _janusDbContext.Users
                .Include(u => u.Repositories) // Load users repositories
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound(new { message = "User not found" });


            // Remove the user
            _janusDbContext.Users.Remove(user);

            await _janusDbContext.SaveChangesAsync();


            // Delete users repo file data
            foreach (var repo in user.Repositories)
            {
                string repoFiles = Path.Combine(fileDir, repo.RepoId.ToString());
                string repoTrees = Path.Combine(treeDir, repo.RepoId.ToString());

                if (Directory.Exists(repoFiles))
                {
                    try
                    {
                        Directory.Delete(repoFiles, true);
                    }
                    catch (Exception ex)
                    {

                    }
                }

                if (Directory.Exists(repoTrees))
                {
                    try
                    {
                        Directory.Delete(repoTrees, true);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            if (!string.IsNullOrEmpty(user.ProfilePicturePath) && System.IO.File.Exists(user.ProfilePicturePath))
            {
                try
                {
                    System.IO.File.Delete(user.ProfilePicturePath);
                }
                catch (Exception ex)
                {

                }
            }

                

            return Ok(new { message = "User deleted successfully" });
        }





    }
}
