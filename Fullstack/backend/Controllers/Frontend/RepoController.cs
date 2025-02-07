using backend.DataTransferObjects;
using backend.Models;
using backend.Services;
using backend.Utils.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace backend.Controllers.Frontend
{
    [Route("api/web/[controller]")]
    [EnableCors("FrontendPolicy")]
    [ApiController]
    [Authorize(Policy = "FrontendPolicy")]
    [EnableRateLimiting("FrontendRateLimit")]
    public class RepoController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;
        private readonly JwtHelper _jwtHelper;
        private RepoManagement _repoManagement;
        private readonly RepoService _repoService;

        public RepoController(JanusDbContext janusDbContext, JwtHelper jwtHelper, RepoManagement repoManagement, RepoService repoService)
        {
            _janusDbContext = janusDbContext;
            _jwtHelper = jwtHelper;
            _repoManagement = repoManagement;
            _repoService = repoService;
        }




        // Create a repo
        // POST: api/web/Repo/Init
        [HttpPost("Init")]
        public async Task<IActionResult> InitRepo([FromBody] RepositoryDto newRepo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            var result = await _repoManagement.InitRepoAsync(
                ownerId: userId,
                repoName: newRepo.RepoName,
                repoDescription: newRepo.RepoDescription,
                isPrivate: newRepo.IsPrivate
            );




            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }






    }
}
