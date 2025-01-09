using backend.DataTransferObjects;
using backend.Helpers;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("CLIPolicy")]
    [Authorize(Policy = "CLIPolicy")]
    public class CLIController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;
        private readonly CLIHelper _cliHelper;

        public CLIController(JanusDbContext janusDbContext, CLIHelper cliHelper)
        {
            _janusDbContext = janusDbContext;
            _cliHelper = cliHelper;
        }


        /*
        // POST: api/CLI/Push
        [HttpPost("Push")]
        public async Task<IActionResult> Push([FromBody] List<CommitDto> commitDtos)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // BranchId TODO
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int userId;
            if (!Int32.TryParse(userIdClaim, out userId))
            {
                return BadRequest(ModelState);
            }

            using (var transaction = await _janusDbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //List<CommitDto> commitDtos = JsonConvert.DeserializeObject<List<CommitDto>>(commitJson);

                    foreach (var commitDto in commitDtos)
                    {
                        // var BranchId

                        var parentCommitId = await _cliHelper.GetParentCommitIdAsync(commitDto.ParentCommitHash);

                        var commit = new Commit
                        {
                            BranchId = 0,
                            UserId = userId,
                            CommitHash = commitDto.CommitHash,
                            Message = commitDto.Message,
                            ParentCommitId = parentCommitId,
                            CommittedAt = commitDto.CommittedAt,
                            Files = commitDto.Files.Select(fileDto => new Models.File
                            {
                                FilePath = fileDto.FilePath,
                                FileHash = fileDto.FileHash,
                                FileContents = new FileContent
                                {
                                    Content = fileDto.FileContent
                                }
                            }).ToList()
                        };

                        // Add to database
                        _janusDbContext.Commits.Add(commit);
                    }



                    await _janusDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { error = ex.Message });
                }
            }


        }
        */

        private class RepoNameBranch
        {
            public string RepoName { get; set; }
            public string BranchName { get; set; }
        }


        // POST: api/CLI/GetHeadCommitHash
        [HttpPost("RemoteHeadCommit")]
        public async Task<IActionResult> RemoteHeadCommit([FromBody] RepoNameBranch repoNameBranch )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(ModelState);
            }

            try
            {
                string repoName = repoNameBranch.RepoName;
                string branchName = repoNameBranch.BranchName;

                // Find the repository for the user
                var repository = await _janusDbContext.Users
                    .Where(User => User.UserId == userId)
                    .SelectMany(User => User.Repositories)
                    .FirstOrDefaultAsync(Repository => Repository.RepoName == repoName);

                if (repository == null)
                {
                    // Create the repository if it doesn't exist
                    repository = new Repository
                    {
                        OwnerId = userId,
                        RepoName = repoName
                    };
                    _janusDbContext.Repositories.Add(repository);
                    await _janusDbContext.SaveChangesAsync();
                    return Ok(new { message = "Created repo" });
                }

                // Find the branch for the repository
                var branch = await _janusDbContext.Branches
                    .Where(Branch => Branch.RepoId == repository.RepoId && Branch.BranchName == branchName)
                    .FirstOrDefaultAsync();

                if (branch == null)
                {
                    // Create the branch if it doesn't exist
                    branch = new Branch
                    {
                        BranchName = branchName,
                        RepoId = repository.RepoId,
                        LatestCommitId = null // or set the default commit ID if available
                    };
                    _janusDbContext.Branches.Add(branch);
                    await _janusDbContext.SaveChangesAsync();

                    return Ok(new { message = $"Created branch '{branchName}' for repo '{repoName}'" });
                }

                // If branch already exists, retrieve the latest commit for that branch
                var latestCommitId = branch.LatestCommitId;

                if (latestCommitId == null)
                {
                    return BadRequest("Couldn't find remote repo's latest commit in the branch");
                }

                var commitHash = await _janusDbContext.Commits
                    .Where(Commit => Commit.CommitId == latestCommitId)
                    .Select(Commit => Commit.CommitHash)
                    .FirstOrDefaultAsync();

                if (commitHash == null)
                {
                    return BadRequest("Couldn't find remote repo's latest commit");
                }

                return Ok(new { CommitHash = commitHash });

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }









                /*
                try
                {
                    string repoName = repoNameBranch.RepoName;
                    string branchName = repoNameBranch.BranchName;

                    var latestCommitId = await _janusDbContext.Users // Find the latest commit for the branch given user, repo name and branch name
                        .Where(User => User.UserId == userId)
                        .SelectMany(User => User.Repositories)
                        .Where(Repository => Repository.RepoName == repoName)
                        .SelectMany(Repository => Repository.Branches)
                        .Where(Branch => Branch.BranchName == branchName)
                        .Select(Branch => Branch.LatestCommitId)
                        .FirstOrDefaultAsync();

                    if (latestCommitId == null)
                    {
                        return BadRequest("Couldn't find remote repos latest commit in the branch");
                    }

                    var commitHash = await _janusDbContext.Commits
                        .Where(Commit => Commit.CommitId == latestCommitId)
                        .Select(Commit => Commit.CommitHash)
                        .FirstOrDefaultAsync();

                    if (commitHash == null)
                    {
                        return BadRequest("Couldn't find remote repos latest commit");
                    }

                    return Ok(new { CommitHash = commitHash });

                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = ex.Message });
                }
                */

            }




        // POST: api/CLI/Push
        [HttpPost("Push")]
        public async Task<IActionResult> Push([FromBody] List<CommitDto> commitDtos)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(ModelState);
            }

            // Use the execution strategy
            var strategy = _janusDbContext.Database.CreateExecutionStrategy();

            try
            {
                // Execute all operations in the execution strategy
                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _janusDbContext.Database.BeginTransactionAsync();

                    try
                    {
                        // Check repo and branch
                        // Send along repo and branch data from local








                        foreach (var commitDto in commitDtos)
                        {
                            var parentCommitId = await _cliHelper.GetParentCommitIdAsync(commitDto.ParentCommitHash);

                            var commit = new Commit
                            {
                                BranchId = 0, // TODO: Set BranchId appropriately
                                UserId = userId,
                                CommitHash = commitDto.CommitHash,
                                Message = commitDto.Message,
                                ParentCommitId = parentCommitId,
                                CommittedAt = commitDto.CommittedAt,
                                Files = commitDto.Files.Select(fileDto => new Models.File
                                {
                                    FilePath = fileDto.FilePath,
                                    FileHash = fileDto.FileHash,
                                    FileContents = new FileContent
                                    {
                                        Content = fileDto.FileContent
                                    }
                                }).ToList()
                            };

                            _janusDbContext.Commits.Add(commit);
                        }

                        await _janusDbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw; // Rethrow the exception to propagate it back to the execution strategy
                    }
                });

                return Ok(); // If everything succeeds, return OK
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message }); // Handle failure and return an appropriate response
            }
        }

    }
}
