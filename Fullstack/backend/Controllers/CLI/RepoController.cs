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

            var cloneData = new CloneDto
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

            var response = new CloneDto
            {
                RepoName = repository.RepoName,
                RepoDescription = repository.RepoDescription,
                IsPrivate = repository.IsPrivate,
                Branches = branchesData
            };


            return Ok(response);
        }














        [HttpPost("janus/{owner}/{repoName}/push")]
        public async Task<IActionResult> PushToRepo(string owner, string repoName, [FromBody] CloneDto pushRequest)
        {
            // Authentication
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Message = "Invalid user" });
            }


            // Get owner and repository
            var ownerUser = await _janusDbContext.Users.FirstOrDefaultAsync(u => u.Username == owner);
            if (ownerUser == null)
                return NotFound(new { Message = "Owner not found" });

            var repository = await _janusDbContext.Repositories
                .Include(r => r.RepoAccesses)
                .Include(r => r.Branches)
                .FirstOrDefaultAsync(r => r.OwnerId == ownerUser.UserId && r.RepoName == repoName);

            if (repository == null)
                return NotFound(new { Message = "Repository not found" });


            // Check write access
            var userAccess = repository.RepoAccesses.FirstOrDefault(ra => ra.UserId == userId);
            if (userAccess == null || userAccess.AccessLevel < AccessLevel.WRITE)
            {
                return Forbid();
            }

            return Ok(new {  Message ="hello"});
            /*

            // Get current users username
            var user = await _janusDbContext.Users.FindAsync(userId);
            string username = user.Username;


            // Find or create branch
            var branch = repository.Branches.FirstOrDefault(b => b.BranchName == pushRequest.BranchName);
            
            if (branch == null)
            {
                // Create new branch
                Branch parentBranch = null;
                if (!string.IsNullOrEmpty(pushRequest.ParentBranchName))
                {
                    parentBranch = repository.Branches.FirstOrDefault(b => b.BranchName == pushRequest.ParentBranchName);
                    if (parentBranch == null)
                        return BadRequest(new { Message = "Parent branch not found" });
                }

                branch = new Branch
                {
                    BranchName = pushRequest.BranchName,
                    RepoId = repository.RepoId,
                    CreatedBy = userId,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ParentBranch = parentBranch?.BranchId,
                    SplitFromCommitHash = parentBranch?.LatestCommitHash ?? pushRequest.SplitFromCommitHash,
                    LatestCommitHash = null
                };
                _janusDbContext.Branches.Add(branch);
                await _janusDbContext.SaveChangesAsync(); // Generate BranchId
            }




            // Fast forward check (skip for new branches)
            if (branch.LatestCommitHash != null && pushRequest.NewCommits.Any())
            {
                // If the remote latest commit is contained in the new commits it can be fast fwd
                var firstNewCommit = pushRequest.NewCommits.First();
                if (!firstNewCommit.ParentCommitHashes.Contains(branch.LatestCommitHash))
                {
                    if (!pushRequest.ForcePush)
                    {
                        return BadRequest(new { Message = "Non fast forward push. Please fetch, pull and merge locally" });
                    }
                }
            }


            // Validate tree hashes
            string treeStoragePath = Environment.GetEnvironmentVariable("TREE_STORAGE_PATH");
            string repoTreeDir = Path.Combine(treeStoragePath, repository.RepoId.ToString());
            foreach (var commitDto in pushRequest.NewCommits)
            {
                string treeFilePath = Path.Combine(repoTreeDir, commitDto.TreeHash);
                


            }



























            // Collect new commit hashes and filter existing ones
            var newCommitHashes = pushRequest.NewCommits.Select(c => c.CommitHash).ToList();
            var existingCommits = await _janusDbContext.Commits
                .Where(c => newCommitHashes.Contains(c.CommitHash))
                .ToDictionaryAsync(c => c.CommitHash);


            


            // Add new commits
            foreach (var commitDto in pushRequest.NewCommits)
            {
                if (!existingCommits.ContainsKey(commitDto.CommitHash))
                {
                    var newCommit = new Commit
                    {
                        CommitHash = commitDto.CommitHash,
                        BranchId = branch.BranchId,
                        TreeHash = commitDto.TreeHash,
                        CreatedBy = username,
                        Message = commitDto.Message,
                        CommittedAt = commitDto.CommittedAt
                    };
                    _janusDbContext.Commits.Add(newCommit);
                }
            }

            // Map CommitHashes to CommitIds
            var commitIds = await _janusDbContext.Commits
                .Where(c => newCommitHashes.Contains(c.CommitHash))
                .ToDictionaryAsync(c => c.CommitHash, c => c.CommitId);

            // Add CommitParents
            foreach (var commitDto in pushRequest.NewCommits)
            {
                var commitId = commitIds[commitDto.CommitHash];
                foreach (var parentHash in commitDto.ParentCommitHashes)
                {
                    if (!commitIds.TryGetValue(parentHash, out int parentId))
                    {
                        var parentCommit = await _janusDbContext.Commits
                            .FirstOrDefaultAsync(c => c.CommitHash == parentHash);
                        if (parentCommit == null)
                        {
                            return BadRequest(new { Message = $"Parent commit {parentHash} not found" });
                        }
                        parentId = parentCommit.CommitId;
                    }

                    if (!await _janusDbContext.CommitParents.AnyAsync(cp => cp.ChildId == commitId && cp.ParentId == parentId))
                    {
                        _janusDbContext.CommitParents.Add(new CommitParent
                        {
                            ChildId = commitId,
                            ParentId = parentId
                        });
                    }
                }
            }

            // Save all trees and files in dirs
            try
            {
                // eviroment variables FILE_STORAGE_PATH, TREE_STORAGE_PATH
            }
            catch
            {
                // Rollback
            }


            // Update branchs latest commit
            if (pushRequest.NewCommits.Any())
            {
                branch.LatestCommitHash = pushRequest.NewCommits.Last().CommitHash;
            }



            await _janusDbContext.SaveChangesAsync();

            */


        }

















    }
}
