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
    public class RepoInvitesController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;

        public RepoInvitesController(JanusDbContext janusDbContext)
        {
            _janusDbContext = janusDbContext;
        }



        [HttpGet]
        public async Task<IActionResult> GetInvites()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var invites = await _janusDbContext.RepoInvites
                .Include(ri => ri.Repository)
                    .ThenInclude(r => r.Owner)
                .Include(ri => ri.Inviter)
                .Where(ri => ri.InviteeUserId == userId)
                .Select(ri => new
                {
                    ri.InviteId,
                    RepoName = ri.Repository.RepoName,
                    OwnerUsername = ri.Repository.Owner.Username,
                    InviterUsername = ri.Inviter.Username,
                    ri.AccessLevel,
                    ri.CreatedAt
                })
                .ToListAsync();

            return Ok(invites);
        }



        [HttpPost("{inviteId}/accept")]
        public async Task<IActionResult> AcceptInvite(int inviteId)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { Message = "Invalid user" });

            var invite = await _janusDbContext.RepoInvites
                .Include(ri => ri.Repository)
                .FirstOrDefaultAsync(ri => ri.InviteId == inviteId &&
                                          ri.InviteeUserId == userId);

            if (invite == null)
                return NotFound();

            // Add access
            _janusDbContext.RepoAccess.Add(new RepoAccess
            {
                RepoId = invite.RepoId,
                UserId = userId,
                AccessLevel = invite.AccessLevel
            });

            // Update invite
            _janusDbContext.RepoInvites.Remove(invite);
            await _janusDbContext.SaveChangesAsync();

            return Ok(new { Message = "Invite accepted" });
        }



        [HttpPost("{inviteId}/decline")]
        public async Task<IActionResult> DeclineInvite(int inviteId)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { Message = "Invalid user" });

            var invite = await _janusDbContext.RepoInvites
                .FirstOrDefaultAsync(ri => ri.InviteId == inviteId &&
                                         ri.InviteeUserId == userId);

            if (invite == null)
                return NotFound();

            _janusDbContext.RepoInvites.Remove(invite);
            await _janusDbContext.SaveChangesAsync();

            return Ok(new { Message = "Invite declined" });
        }



    }
}

