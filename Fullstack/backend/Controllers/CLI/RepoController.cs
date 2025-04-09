using backend.DataTransferObjects.CLI;
using backend.Helpers;
using backend.Models;
using backend.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using System.Text.Json;
using static backend.Utils.TreeBuilder;

namespace backend.Controllers.CLI
{
    [Route("api/cli/[controller]")]
    [ApiController]
    [EnableCors("CLIPolicy")]
    [Authorize(Policy = "CLIPolicy")]
    [EnableRateLimiting("CLIRateLimit")]
    public class RepoController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;
        private readonly CLIHelper _cliHelper;

        public RepoController(JanusDbContext janusDbContext, CLIHelper cliHelper)
        {
            _janusDbContext = janusDbContext;
            _cliHelper = cliHelper;
        }


        // GET: api/CLI/repo/janus/{owner}/{repoName}
        [HttpGet("janus/{owner}/{repoName}")]
        public async Task<IActionResult> CloneRepo(string owner, string repoName)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Message = "Invalid user" });
            }


            // Get the owner of the repo
            var ownerUser = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Username == owner);
            if (ownerUser == null)
                return NotFound(new { Message = "Owner not found" });


            // Get the repo of the owner
            var repository = await _janusDbContext.Repositories
                .Include(r => r.RepoAccesses)
                .Include(r => r.Branches)
                    .ThenInclude(b => b.Parent)
                .Include(r => r.Branches)
                    .ThenInclude(b => b.Commits)
                        .ThenInclude(c => c.Parents)
                            .ThenInclude(cp => cp.Parent)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.OwnerId == ownerUser.UserId && r.RepoName == repoName);

            if (repository == null)
                return NotFound(new { Message = "Repository not found" });


            // Private repos need access to the repo
            if (repository.IsPrivate && !repository.RepoAccesses.Any(ra => ra.UserId == userId))
                return NotFound(new { Message = "Repository not found" }); // Repository is hidden, mask unauthorised with not found error


            /*
                Get the clone data
                (Repo details, Branches, Commits, Trees, File Names/Hashes)
                Exclude file content
            */

            var treeBuilder = new TreeBuilder(repository.RepoId);
            var branchesData = new List<BranchDto>();

            foreach (var branch in repository.Branches)
            {
                branchesData.Add(await _cliHelper.GetBranchDataAsync(branch, treeBuilder));
            }

            var cloneData = new RepoDto
            {
                RepoName = repository.RepoName,
                RepoDescription = repository.RepoDescription,
                IsPrivate = repository.IsPrivate,
                Branches = branchesData
            };

            return Ok(cloneData);
        }




        // POST: api/CLI/batchfiles/{owner}/{repoName}
        [HttpPost("batchfiles/{owner}/{repoName}")]
        public async Task<IActionResult> GetBatchFileContent(string owner, string repoName, [FromBody] List<string> fileHashes)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Message = "Invalid user" });
            }


            // Get the owner of the repo
            var ownerUser = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Username == owner);
            if (ownerUser == null)
                return NotFound(new { Message = "Owner not found" });


            // Get the repo of the owner
            var repository = await _janusDbContext.Repositories
                .Include(r => r.RepoAccesses)
                .FirstOrDefaultAsync(r =>
                    r.OwnerId == ownerUser.UserId && r.RepoName == repoName);

            if (repository == null)
                return NotFound(new { Message = "Repository not found" });


            // Private repos need access to the repo
            if (repository.IsPrivate && !repository.RepoAccesses.Any(ra => ra.UserId == userId))
                return NotFound(new { Message = "Repository not found" }); // Repository is hidden, mask unauthorised with not found error


            string fileDir = Path.Combine(Environment.GetEnvironmentVariable("FILE_STORAGE_PATH"), repository.RepoId.ToString());

            // Generate unique boundary without dashes
            string boundary = "boundary_" + Guid.NewGuid().ToString("N");

            // Create a multipart content with a unique boundary for files
            var multipartContent = new MultipartContent("mixed", boundary);

            var missingFiles = new List<string>();

            foreach (var hash in fileHashes)
            {
                string filePath = Path.Combine(fileDir, hash);
                if (!System.IO.File.Exists(filePath))
                {
                    missingFiles.Add(filePath);
                    continue;
                }

                // Create stream content for the file
                var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var streamContent = new StreamContent(fileStream);

                // Add file hash to the header
                streamContent.Headers.Add("X-File-Hash", hash);

                multipartContent.Add(streamContent);
            }

            if (missingFiles.Any())
            {
                // Adds the missing files to the response headers
                Response.Headers.Add("X-Missing-Files", JsonSerializer.Serialize(missingFiles));
            }


            // return a multipart content async stream
            return new FileStreamResult(await multipartContent.ReadAsStreamAsync(), $"multipart/mixed; boundary={boundary}");
        }














        [HttpPost("janus/{owner}/{repoName}/fetch")]
        public async Task<IActionResult> FetchRepo(string owner, string repoName, [FromBody] Dictionary<string, string> latestBranchHashes)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Message = "Invalid user" });
            }

            // Get repository owner
            var ownerUser = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Username == owner);
            if (ownerUser == null)
                return NotFound(new { Message = "Owner not found" });

            // Get repository with branches and commits
            var repository = await _janusDbContext.Repositories
                .Include(r => r.RepoAccesses)
                .Include(r => r.Branches)
                    .ThenInclude(b => b.Commits)
                        .ThenInclude(c => c.Parents)
                            .ThenInclude(cp => cp.Parent)
                .AsSplitQuery()
                .FirstOrDefaultAsync(r => r.OwnerId == ownerUser.UserId && r.RepoName == repoName);

            if (repository == null)
                return NotFound(new { Message = "Repository not found" });

            // Check access for private repos
            if (repository.IsPrivate && !repository.RepoAccesses.Any(ra => ra.UserId == userId))
                return NotFound(new { Message = "Repository not found" });



            var treeBuilder = new TreeBuilder(repository.RepoId);
            var branchesData = new List<BranchDto>();

            foreach (var branch in repository.Branches)
            {
                List<Commit> newCommits = new List<Commit>();

                // Check if the client has the branch
                bool clientHasBranch = latestBranchHashes.TryGetValue(branch.BranchName, out string clientLatestHash);

                if (clientHasBranch)
                {

                    if (clientLatestHash == branch.LatestCommitHash)
                    {
                        // Client branch is up to date -> no new commits
                        newCommits = new List<Commit>();
                    }
                    else
                    {
                        // Client has the branch but not the latest commit
                        var remoteLatestCommit = branch.Commits.FirstOrDefault(c => c.CommitHash == branch.LatestCommitHash);

                        // Use BFS to traverse all parents of each commit
                        newCommits = new List<Commit>();
                        var visited = new HashSet<string>();
                        var queue = new Queue<Commit>();

                        queue.Enqueue(remoteLatestCommit);

                        bool clientCommitFound = false;

                        while (queue.Count > 0)
                        {
                            var currentCommit = queue.Dequeue();

                            // Skip if already visited
                            if (visited.Contains(currentCommit.CommitHash))
                                continue;

                            visited.Add(currentCommit.CommitHash);

                            // Stop if we reach the clients latest commit
                            if (currentCommit.CommitHash == clientLatestHash)
                            {
                                clientCommitFound = true;
                                break;
                            }

                            newCommits.Add(currentCommit);

                            // Enqueue all parents
                            foreach (var parent in currentCommit.Parents.Select(cp => cp.Parent))
                            {
                                queue.Enqueue(parent);
                            }
                        }

                        if (!clientCommitFound)
                        {
                            // Divergence -> send all commits
                            newCommits = branch.Commits.ToList();
                        }
                    }

                }
                else
                {
                    // Client does not have the branch -> send all commits
                    newCommits = branch.Commits.ToList();
                }

                var commitDtos = await _cliHelper.GetCommitDtosAsync(newCommits, treeBuilder);

                var createdByUser = await _janusDbContext.Users
                    .Where(u => u.UserId == branch.CreatedBy)
                    .Select(u => u.Username)
                    .FirstOrDefaultAsync();

                branchesData.Add(new BranchDto
                {
                    BranchName = branch.BranchName,
                    ParentBranch = branch.Parent?.BranchName,
                    SplitFromCommitHash = branch.SplitFromCommitHash,
                    LatestCommitHash = branch.LatestCommitHash,
                    CreatedBy = createdByUser,
                    Created = branch.CreatedAt,
                    Commits = commitDtos
                });
            }

            var response = new RepoDto
            {
                RepoName = repository.RepoName,
                RepoDescription = repository.RepoDescription,
                IsPrivate = repository.IsPrivate,
                Branches = branchesData
            };


            return Ok(response);
        }














        [HttpGet("janus/{owner}/{repoName}/head")]
        public async Task<IActionResult> GetRemoteHead(string owner, string repoName)
        {
            // Authentication
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Message = "Invalid user" });
            }


            // Get repository owner
            var ownerUser = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Username == owner);
            if (ownerUser == null)
                return NotFound(new { Message = "Owner not found" });


            // Get repository with branches and commits
            var repository = await _janusDbContext.Repositories
                .Include(r => r.RepoAccesses)
                .Include(r => r.Branches)
                .AsSplitQuery()
                .FirstOrDefaultAsync(r => r.OwnerId == ownerUser.UserId && r.RepoName == repoName);

            if (repository == null)
                return NotFound(new { Message = "Repository not found" });

            // Check access for private repos
            if (repository.IsPrivate && !repository.RepoAccesses.Any(ra => ra.UserId == userId))
                return NotFound(new { Message = "Repository not found" });




            // Get branch names and their latest commit hash
            var branchHeads = repository.Branches.ToDictionary(b => b.BranchName, b => b.LatestCommitHash);

            var remoteHeadDto = new RemoteHeadDto
            {
                Description = repository.RepoDescription,
                IsPrivate = repository.IsPrivate,
                Heads = branchHeads
            };

            return Ok(remoteHeadDto);
        }






















        [HttpPost("janus/{owner}/{repoName}/push")]
        public async Task<IActionResult> PushToRepo(string owner, string repoName)
        {
            // Authentication
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Message = "Invalid user" });
            }


            // Ensure the request is a multipart/form-data request.
            if (!Request.HasFormContentType)
            {
                return BadRequest("Expected a multipart/form-data request.");
            }

            // Read the multipart form data
            var form = await Request.ReadFormAsync();

            // Get the metadata
            if (!form.TryGetValue("metadata", out var metadataValues))
            {
                return BadRequest("Missing metadata.");
            }

            RepoDto pushRequest;
            try
            {
                pushRequest = JsonSerializer.Deserialize<RepoDto>(metadataValues.First());
            }
            catch (Exception ex)
            {
                return BadRequest($"Invalid metadata: {ex.Message}");
            }





            // Get owner and repository
            var ownerUser = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Username == owner);
            if (ownerUser == null)
                return NotFound(new { Message = "Owner not found" });

            var repository = await _janusDbContext.Repositories
                .Include(r => r.RepoAccesses)
                .Include(r => r.Branches)
                    .ThenInclude(b => b.Commits)
                .FirstOrDefaultAsync(r => r.OwnerId == ownerUser.UserId && r.RepoName == repoName);

            if (repository == null)
                return NotFound(new { Message = "Repository not found" });


            // Check write access
            var userAccess = repository.RepoAccesses.FirstOrDefault(ra => ra.UserId == userId);
            if (userAccess == null || userAccess.AccessLevel < AccessLevel.WRITE)
            {
                return Forbid();
            }


            






            var strategy = _janusDbContext.Database.CreateExecutionStrategy();
            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _janusDbContext.Database.BeginTransactionAsync();

                    try
                    {
                        var user = await _janusDbContext.Users.FindAsync(userId);
                        string username = user.Username;



                        // Update repository metadata
                        repository.RepoDescription = pushRequest.RepoDescription;
                        repository.IsPrivate = pushRequest.IsPrivate;
                        _janusDbContext.Update(repository);



                        // Branch
                        var branchDto = pushRequest.Branches.FirstOrDefault();
                        if (branchDto == null)
                            throw new InvalidOperationException("No branch provided");

                        var existingBranch = await _janusDbContext.Branches
                            .Include(b => b.Commits)
                            .FirstOrDefaultAsync(b => b.RepoId == repository.RepoId && b.BranchName == branchDto.BranchName);


                        if (existingBranch == null)
                        {
                            // Create branch
                            existingBranch = new Branch
                            {
                                BranchName = branchDto.BranchName,
                                RepoId = repository.RepoId,
                                CreatedBy = userId,
                                CreatedAt = DateTimeOffset.UtcNow,
                                SplitFromCommitHash = branchDto.SplitFromCommitHash,
                                LatestCommitHash = branchDto.LatestCommitHash,
                            };


                            if (string.IsNullOrEmpty(branchDto.ParentBranch))
                            {
                                // Should be main branch
                                if (!branchDto.BranchName.Equals("main", StringComparison.OrdinalIgnoreCase))
                                {
                                    throw new InvalidOperationException("Invalid parent branch");
                                }
                            }
                            else
                            {
                                // Other branches

                                var parentBranch = await _janusDbContext.Branches
                                    .FirstOrDefaultAsync(b => b.RepoId == repository.RepoId && b.BranchName == branchDto.ParentBranch);
                                if (parentBranch == null)
                                {
                                    // Rollback
                                    throw new InvalidOperationException("Parent branch not found");
                                }
                                existingBranch.ParentBranch = parentBranch.BranchId;
                            }

                            _janusDbContext.Branches.Add(existingBranch);
                            await _janusDbContext.SaveChangesAsync();

                        }
                        else
                        {
                            // Check branch can be fast forwarded
                            if (existingBranch.LatestCommitHash != branchDto.SplitFromCommitHash)
                            {
                                // Rollback
                                throw new InvalidOperationException("Non fast forward push");
                            }
                        }




                        // Commits
                        foreach (var commitDto in branchDto.Commits)
                        {
                            //Skip commits that already exist
                            if (await _janusDbContext.Commits.AnyAsync(c => c.CommitHash == commitDto.CommitHash))
                                continue;


                            // Validate tree
                            var hashes = new HashSet<string>();
                            if (commitDto.Tree != null)
                            {
                                var treeBuilder = new TreeBuilder(repository.RepoId);
                                treeBuilder.LoadTree(commitDto.Tree);
                                string savedTreeHash = treeBuilder.SaveTree();
                            

                                // Files to compare with files comming in
                                treeBuilder.GetFileHashes(hashes);
                            }


                            // Create commit
                            var newCommit = new Commit
                            {
                                CommitHash = commitDto.CommitHash,
                                BranchId = existingBranch.BranchId,
                                TreeHash = commitDto.TreeHash ?? "",
                                CreatedBy = username,
                                Message = commitDto.Message,
                                CommittedAt = commitDto.CommittedAt
                            };

                            _janusDbContext.Commits.Add(newCommit);
                            await _janusDbContext.SaveChangesAsync();


                            // Commit Parents
                            foreach (var parentHash in commitDto.ParentsCommitHash)
                            {
                                var parentCommit = await _janusDbContext.Commits.FirstOrDefaultAsync(c => c.CommitHash == parentHash);

                                if (parentCommit == null)
                                {
                                    // Rollback
                                    throw new InvalidOperationException("Non fast forward push");
                                }

                                _janusDbContext.CommitParents.Add(new CommitParent
                                {
                                    ChildId = newCommit.CommitId,
                                    ParentId = parentCommit.CommitId
                                });
                            }
                        }



                        // Update branchs latest commit hash
                        if (branchDto.Commits.Any())
                        {
                            existingBranch.LatestCommitHash = branchDto.LatestCommitHash;
                            _janusDbContext.Branches.Update(existingBranch);
                        }


                        // Save files that come with the request
                        string fileDir = Path.Combine(Environment.GetEnvironmentVariable("FILE_STORAGE_PATH"), repository.RepoId.ToString());
                        Directory.CreateDirectory(fileDir);

                        foreach (var file in form.Files)
                        {
                            // Header X-File-Hash for each file
                            string fileHash = file.FileName;

                            string filePath = Path.Combine(fileDir, fileHash);
                            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                            {
                                await file.CopyToAsync(stream);
                            }
                        }

                        await _janusDbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw ex;
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occured: {ex}");
            }
           

            return Ok(new { Message = "Push successful" });

        }

















    }
}
