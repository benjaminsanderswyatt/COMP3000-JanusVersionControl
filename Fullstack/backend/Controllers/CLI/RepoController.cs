using backend.DataTransferObjects.CLI;
using backend.Helpers;
using backend.Models;
using backend.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
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
            var branchesData = new List<object>();

            foreach (var branch in repository.Branches)
            {
                branchesData.Add(await _cliHelper.GetBranchDataAsync(branch, treeBuilder));
            }

            var cloneData = new
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

















        
        [HttpPost("fetch/{owner}/{repoName}")]
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


            /*
                For each branch:
                If the branch is not included in the clients latestBranchHashes -> return the full branch history
                Otherwise start from the branchs latest commit and traverse commit parents until the clients known commit is found
                If the clients commit is not found assume a divergence and send the complete branch history
            */

            // Response object
            var response = new
            {
                BranchLatestHashes = new Dictionary<string, string>(),
                NewBranches = new List<object>(),
                NewCommits = new Dictionary<string, List<object>>()
            };

            var treeBuilder = new TreeBuilder(repository.RepoId);

            foreach (var branch in repository.Branches)
            {

                if (!latestBranchHashes.TryGetValue(branch.BranchName, out string clientLatestHash))
                {
                    // Get all the branch data
                    var branchData = await _cliHelper.GetBranchDataAsync(branch, treeBuilder);

                    response.NewBranches.Add(branchData);

                }
                else
                {
                    
                    var remoteLatestHash = branch.LatestCommitHash;
                    if (clientLatestHash == remoteLatestHash) // Clients branch is up to date
                        continue;

                    var remoteLatestCommit = branch.Commits.FirstOrDefault(c => c.CommitHash == remoteLatestHash);
                    if (remoteLatestCommit == null) // Get latest remote commit
                        continue;

                    // Use BFS to traverse all parents of each commit
                    var newCommits = new List<Commit>();
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
                        // Divergance send all commits from remote
                        newCommits = branch.Commits.ToList();
                    }

                    // Get commit dtos
                    var commitDtos = await _cliHelper.GetCommitDtosAsync(newCommits, treeBuilder);
                    if (commitDtos.Any())
                    {
                        response.NewCommits[branch.BranchName] = commitDtos;
                    }

                }

                // Save the branches latest commits hashes
                response.BranchLatestHashes[branch.BranchName] = branch.LatestCommitHash;
            }

            return Ok(response);
        }
        



        













    }
}
