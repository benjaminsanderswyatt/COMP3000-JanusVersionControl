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
    public class BranchController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;

        public BranchController(JanusDbContext janusDbContext)
        {
            _janusDbContext = janusDbContext;
        }



        // POST: api/Branch/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] BranchDto branchDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { error = "Invalid or missing user" });
            }


            // Check if the repository exists for the user
            var repository = await _janusDbContext.Repositories
                .FirstOrDefaultAsync(r => r.RepoId == branchDto.RepoId && r.OwnerId == userId);

            if (repository == null)
            {
                return NotFound(new { error = "Repository not found or not owned by the user" });
            }


            // Check if branch name already exists in repo
            var existingBranch = await _janusDbContext.Branches
                .FirstOrDefaultAsync(b => b.BranchName == branchDto.BranchName && b.RepoId == branchDto.RepoId);

            if (existingBranch != null)
            {
                return Conflict(new { error = $"A branch with the name '{branchDto.BranchName}' already exists in this repository." });
            }


            var strategy = _janusDbContext.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _janusDbContext.Database.BeginTransactionAsync();

                    try
                    {
                        // Create new branch
                        var newBranch = new Branch
                        {
                            RepoId = branchDto.RepoId, // Link branch to the repo
                            BranchName = branchDto.BranchName,
                            CreatedAt = branchDto.CreatedAt,
                            LatestCommitId = branchDto.LatestCommitId
                        };

                        _janusDbContext.Branches.Add(newBranch);

                        await _janusDbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return Ok(new { message = "Branch created successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
