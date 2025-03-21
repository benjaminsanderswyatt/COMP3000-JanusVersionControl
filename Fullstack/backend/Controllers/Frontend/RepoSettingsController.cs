﻿using backend.Models;
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
    public class RepoSettingsController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;

        public RepoSettingsController(JanusDbContext janusDbContext)
        {
            _janusDbContext = janusDbContext;
        }

        public class EditDescriptionDto
        {
            public string Description { get; set; }
        }

        public class ChangeVisibilityDto
        {
            public bool IsPrivate { get; set; }
        }






        // Update repository description
        [HttpPut("{owner}/{repoName}/description")]
        public async Task<IActionResult> EditDescription(string owner, string repoName, [FromBody] EditDescriptionDto dto)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { Message = "Invalid user" });

            var ownerUser = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Username == owner);
            if (ownerUser == null)
                return NotFound(new { Message = "Owner not found" });

            var repo = await _janusDbContext.Repositories.FirstOrDefaultAsync(r =>
                r.OwnerId == ownerUser.UserId && r.RepoName == repoName);
            if (repo == null)
                return NotFound(new { Message = "Repository not found" });


            repo.RepoDescription = dto.Description;
            await _janusDbContext.SaveChangesAsync();

            return Ok(new { Message = "Repository description updated", Description = repo.RepoDescription });
        }


        // Toggle repository visibility
        [HttpPut("{owner}/{repoName}/visibility")]
        public async Task<IActionResult> ChangeVisibility(string owner, string repoName, [FromBody] ChangeVisibilityDto dto)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { Message = "Invalid user" });

            var ownerUser = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Username == owner);
            if (ownerUser == null)
                return NotFound(new { Message = "Owner not found" });

            var repo = await _janusDbContext.Repositories.FirstOrDefaultAsync(r =>
                r.OwnerId == ownerUser.UserId && r.RepoName == repoName);
            if (repo == null)
                return NotFound(new { Message = "Repository not found" });



            repo.IsPrivate = dto.IsPrivate;
            await _janusDbContext.SaveChangesAsync();

            return Ok(new { Message = "Repository visibility updated", IsPrivate = repo.IsPrivate });
        }



        // Delete repository
        [HttpDelete("{owner}/{repoName}")]
        public async Task<IActionResult> DeleteRepository(string owner, string repoName)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { Message = "Invalid user" });

            var ownerUser = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Username == owner);
            if (ownerUser == null)
                return NotFound(new { Message = "Owner not found" });

            var repo = await _janusDbContext.Repositories.FirstOrDefaultAsync(r =>
                r.OwnerId == ownerUser.UserId && r.RepoName == repoName);
            if (repo == null)
                return NotFound(new { Message = "Repository not found" });


            _janusDbContext.Repositories.Remove(repo);
            await _janusDbContext.SaveChangesAsync();



            // Delete the repo files
            string fileDir = Path.Combine(Environment.GetEnvironmentVariable("FILE_STORAGE_PATH"), repo.RepoId.ToString());
            string treeDir = Path.Combine(Environment.GetEnvironmentVariable("TREE_STORAGE_PATH"), repo.RepoId.ToString());

            if (Directory.Exists(fileDir))
            {
                try
                {
                    Directory.Delete(fileDir, true);
                }
                catch (Exception ex)
                {

                }
            }

            if (Directory.Exists(treeDir))
            {
                try
                {
                    Directory.Delete(treeDir, true);
                }
                catch (Exception ex)
                {

                }
            }




            return Ok(new { Message = "Repository deleted successfully" });
        }


    }
}
