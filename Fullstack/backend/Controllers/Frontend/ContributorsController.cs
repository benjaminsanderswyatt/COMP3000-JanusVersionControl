using backend.Models;
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
                    Email = access.User.Email,
                    AccessLevel = access.AccessLevel.ToString()
                });
            }


            return Ok(new { contributors });

        }





        [HttpPost("{owner}/{repoName}/invite")]
        public async Task<IActionResult> SendInvite(string owner, string repoName, [FromBody] InviteRequest request)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { Message = "Invalid user" });

            // Get repo with owner
            var repo = await _janusDbContext.Repositories
                .Include(r => r.Owner)
                .Include(r => r.RepoAccesses)
                .FirstOrDefaultAsync(r => r.Owner.Username == owner && r.RepoName == repoName);

            if (repo == null)
                return NotFound(new { Message = "Repository not found" });


            // Get invitee user
            var invitee = await _janusDbContext.Users
                .FirstOrDefaultAsync(u => u.Username == request.InviteeUsername);
            if (invitee == null)
                return NotFound(new { Message = "User not found" });

            // Check permissions
            var inviterAccess = repo.RepoAccesses.FirstOrDefault(ra => ra.UserId == userId);
            if (inviterAccess == null ||
               (inviterAccess.AccessLevel != AccessLevel.OWNER && inviterAccess.AccessLevel != AccessLevel.ADMIN))
                return Forbid();

            // Check if user already has access or pending invite
            bool hasAccess = repo.RepoAccesses.Any(ra => ra.UserId == invitee.UserId);
            bool existingInvite = await _janusDbContext.RepoInvites
                .AnyAsync(ri => ri.RepoId == repo.RepoId &&
                              ri.InviteeUserId == invitee.UserId);

            if (hasAccess || existingInvite)
                return BadRequest(new { Message = "User already has access or pending invite" });



            // Create invite
            var invite = new RepoInvite
            {
                RepoId = repo.RepoId,
                InviterUserId = userId,
                InviteeUserId = invitee.UserId,
                AccessLevel = request.AccessLevel
            };

            _janusDbContext.RepoInvites.Add(invite);
            await _janusDbContext.SaveChangesAsync();

            return Ok(new { Message = "Invite sent" });
        }

        public class InviteRequest
        {
            public string InviteeUsername { get; set; }
            public AccessLevel AccessLevel { get; set; }
        }



        [HttpDelete("{owner}/{repoName}/leave")]
        public async Task<IActionResult> LeaveRepository(string owner, string repoName)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { Message = "Invalid user" });

            // Get repository
            var repo = await _janusDbContext.Repositories
                .Include(r => r.Owner)
                .Include(r => r.RepoAccesses)
                .FirstOrDefaultAsync(r => r.Owner.Username == owner && r.RepoName == repoName);

            if (repo == null)
                return NotFound(new { Message = "Repository not found" });

            // Check if user is owner
            if (repo.OwnerId == userId)
                return BadRequest(new { Message = "Owners cannot leave repositories" });

            // Find users access
            var userAccess = repo.RepoAccesses.FirstOrDefault(ra => ra.UserId == userId);
            if (userAccess == null)
                return NotFound(new { Message = "You are not a contributor" });

            // Remove access
            _janusDbContext.RepoAccess.Remove(userAccess);
            await _janusDbContext.SaveChangesAsync();

            return Ok(new { Message = "Left repository successfully" });
        }




    }
}
