using backend.Models;
using backend.Services;
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
    public class CommitController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;

        public CommitController(JanusDbContext janusDbContext)
        {
            _janusDbContext = janusDbContext;
        }



        // ------- COMMIT PAGE -------
        [HttpGet("{owner}/{repoName}/{branch}/commits")]
        public async Task<IActionResult> GetCommits(
            string owner,
            string repoName,
            string branch,
            [FromQuery] string? startHash,
            [FromQuery] int limit = 20
            )
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { Message = "Invalid user" });


            // Get the repository owner
            var ownerUser = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Username == owner);
            if (ownerUser == null)
                return NotFound(new { Message = "Owner not found" });


            // Find the repository by owner and name
            var repository = await _janusDbContext.Repositories
                .Include(r => r.RepoAccesses)
                .FirstOrDefaultAsync(r => r.OwnerId == ownerUser.UserId && r.RepoName == repoName);

            if (repository == null)
                return NotFound(new { Message = "Repository not found" });


            // Check access for private repositories
            if (repository.IsPrivate && !repository.RepoAccesses.Any(ra => ra.UserId == userId))
                return NotFound(new { Message = "Repository not found" });


            // Get the requested branch
            var targetBranch = await _janusDbContext.Branches
                .FirstOrDefaultAsync(b => b.RepoId == repository.RepoId && b.BranchName == branch);
            if (targetBranch == null)
                return NotFound(new { Message = "Branch not found" });



            // Get the starting commit hash (null uses latest hash)
            string currentCommitHash = string.IsNullOrEmpty(startHash) ? targetBranch.LatestCommitHash : startHash;

            if (string.IsNullOrEmpty(currentCommitHash))
                return NotFound(new { Message = "No commits found in this branch" });







            // Traverse the commit chain using the parent relationship
            var commitsChain = new List<object>();
            int count = 0;


            // Get the first commit
            var currentCommit = await _janusDbContext.Commits
                .FirstOrDefaultAsync(c => c.CommitHash == currentCommitHash && c.BranchId == targetBranch.BranchId);


            while (currentCommit != null && count < limit)
            {
                // Map the commit to the response DTO
                int commitUserId = 0;
                if (currentCommit.CreatedBy != "Janus")
                {
                    // Look up the user by username
                    var commitUser = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Username == currentCommit.CreatedBy);
                    commitUserId = commitUser?.UserId ?? 0;
                }

                var commitDto = new
                {
                    userId = commitUserId,
                    userName = currentCommit.CreatedBy,
                    message = currentCommit.Message,
                    hash = currentCommit.CommitHash,
                    date = currentCommit.CommittedAt
                };

                commitsChain.Add(commitDto);
                count++;

                // Follow the parent link
                var parentLink = await _janusDbContext.CommitParents
                    .FirstOrDefaultAsync(cp => cp.ChildId == currentCommit.CommitId);

                if (parentLink == null)
                {
                    currentCommit = null;
                    break;
                }

                currentCommit = await _janusDbContext.Commits
                    .FirstOrDefaultAsync(c => c.CommitId == parentLink.ParentId);
            }

            // The next cursor is the hash of the next commit in the chain (if available)
            string? nextCursor = currentCommit != null ? currentCommit.CommitHash : null;

            return Ok(new { commits = commitsChain, nextCursor });

        }






    }
}
