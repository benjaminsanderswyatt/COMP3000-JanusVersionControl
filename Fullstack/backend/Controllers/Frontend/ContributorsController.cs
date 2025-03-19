using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace backend.Controllers.Frontend
{
    [Route("api/web/[controller]")]
    [EnableCors("FrontendPolicy")]
    [ApiController]
    [Authorize(Policy = "FrontendPolicy")]
    [EnableRateLimiting("FrontendRateLimit")]
    public class ContributorsController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;

        public ContributorsController(JanusDbContext janusDbContext)
        {
            _janusDbContext = janusDbContext;
        }



        // ------- CONTRIBUTORS PAGE -------
        [HttpGet("{owner}/{repoName}/contributors")]
        public async Task<IActionResult> GetCommits(string owner, string repoName)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { Message = "Invalid user" });


            // Get the repository owner
            var ownerUser = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Username == owner);
            if (ownerUser == null)
                return NotFound(new { Message = "Owner not found" });


            // Find the repository by owner and name
            var repo = await _janusDbContext.Repositories
                .Include(r => r.Owner)
                .Include(r => r.RepoAccesses)
                    .ThenInclude(ra => ra.User)
                .FirstOrDefaultAsync(r => r.OwnerId == ownerUser.UserId && r.RepoName == repoName);

            if (repo == null)
                return NotFound(new { Message = "Repository not found" });


            // Check access for private repositories
            if (repo.IsPrivate && !repo.RepoAccesses.Any(ra => ra.UserId == userId))
                return NotFound(new { Message = "Repository not found" });





            var contributors = new List<object>();

            foreach (var access in repo.RepoAccesses)
            {
                contributors.Add(new
                {
                    UserId = access.User.UserId,
                    Username = access.User.Username,
                    AccessLevel = access.AccessLevel.ToString()
                });
            }


            return Ok(new { contributors });

        }






    }
}
