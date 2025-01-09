using backend.DataTransferObjects;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CLIPolicy")]
    [Authorize(Policy = "CLIPolicy")]
    public class RepoController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;

        public RepoController(JanusDbContext janusDbContext)
        {
            _janusDbContext = janusDbContext;
        }



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



    }
}
