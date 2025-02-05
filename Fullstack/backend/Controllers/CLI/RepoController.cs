using backend.DataTransferObjects;
using backend.Models;
using backend.Services;
using backend.Utils.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers.CLI
{
    [Route("api/cli/[controller]")]
    [ApiController]
    [EnableCors("CLIPolicy")]
    [Authorize(Policy = "CLIPolicy")]
    public class RepoController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;
        private RepoManagement _repoManagement;
        private readonly RepoService _repoService;

        public RepoController(JanusDbContext janusDbContext, RepoManagement repoManagement, RepoService repoService)
        {
            _janusDbContext = janusDbContext;
            _repoManagement = repoManagement;
            _repoService = repoService;
        }


        // Get all repos of user
        [HttpGet("GetRepos")]
        public async Task<IActionResult> GetUserRepos()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(ModelState);
            }

            try
            {
                var repos = await _repoManagement.GetAllReposOfUserAsync(userId);

                return Ok(repos);

            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while retrieving repositories");
            }
        }


        // Create a repo
        [HttpPost("Create")]
        public async Task<IActionResult> CreateRepo([FromBody] RepositoryDto newRepo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            var result = await _repoService.CreateRepoAsync(userId, newRepo);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        // Delete repo
        [HttpDelete("{repoId}")]
        public async Task<IActionResult> DeleteRepo(int repoId)
        {
            return BadRequest();
        }


















        /*
        // POST: api/Repo/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] RepositoryDto repositoryDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid or missing user" });
            }

            var strategy = _janusDbContext.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _janusDbContext.Database.BeginTransactionAsync();

                    try
                    {
                        // Create new repository
                        var newRepo = new Repository
                        {
                            RepoName = repositoryDto.RepoName,
                            OwnerId = userId,
                            CreatedAt = repositoryDto.CreatedAt
                        };

                        _janusDbContext.Repositories.Add(newRepo);

                        await _janusDbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return Ok(new { message = "Repository created successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }



        // DELETE: api/Repo/Delete/{repoId}
        [HttpDelete("Delete/{repoId}")]
        public async Task<IActionResult> Delete(int repoId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid or missing user" });
            }

            // Find the repository by ID and ensure it belongs to the current user
            var repo = await _janusDbContext.Repositories
                .FirstOrDefaultAsync(r => r.RepoId == repoId && r.OwnerId == userId);

            if (repo == null)
            {
                return NotFound(new { error = "Repository not found or you do not have permission to delete it." });
            }


            var strategy = _janusDbContext.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _janusDbContext.Database.BeginTransactionAsync();

                    try
                    {
                        // Remove the repository (cascade)
                        _janusDbContext.Repositories.Remove(repo);

                        await _janusDbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return Ok(new { message = "Repository deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }



        }

        */

    }
}
