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

    }
}
