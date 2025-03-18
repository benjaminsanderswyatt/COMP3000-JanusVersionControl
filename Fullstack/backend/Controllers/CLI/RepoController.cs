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


        // POST: api/CLI/repo/SayHello
        [HttpPost("SayHello")]
        public async Task<IActionResult> SayHello()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(ModelState);
            }

            try
            {
                Console.WriteLine($"hello: {userId}");

                return Ok(new { message = $"hello {userId}" });

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }





        // POST: api/CLI/Init

        // GET: api/CLI/repo/{owner}/{repoName}/LatestCommit
        [HttpGet("{owner}/{repoName}/{branch}/latestcommit")]
        public async Task<IActionResult> GetLatestCommit(string owner, string repoName, string branch)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { Message = "Invalid user" });
            }

            // Get user by username
            var ownerUser = await _janusDbContext.Users
                .FirstOrDefaultAsync(u => u.Username == owner);

            if (ownerUser == null)
            {
                return NotFound(); // Generic not found hides existance of user
            }


            // Find the repo of the owner
            var repo = await _janusDbContext.Repositories
                .FirstOrDefaultAsync(r => r.OwnerId == ownerUser.UserId && r.RepoName == repoName);

            if (repo == null)
            {
                return NotFound(); // Generic not found hides existance of repo
            }

            // Ensure the user has access to this repo
            var hasAccess = !repo.IsPrivate || // Public repo
                await _janusDbContext.RepoAccess.AnyAsync(ra => ra.UserId == userId && ra.RepoId == repo.RepoId);

            if (!hasAccess)
            {
                return NotFound(); // Generic not found hides existance of repo
            }

            /*
            // Get the latest commit hash for branch
            var branchEntity = await _janusDbContext.Branches
                .Where(b => b.RepoId == repo.RepoId && b.BranchName == branch)
                .Select(b => new { b.LatestCommitHash })
                .FirstOrDefaultAsync();

            if (branchEntity == null)
            {
                // Branch doesnt exist so return empty (no commits for the branch)
                return Ok(new { LatestCommit = "" });
            }

            return Ok(new { LatestCommit = branchEntity.LatestCommitHash });
            */
            return BadRequest();
        }


        // POST: api/CLI/Push
        /*
        [HttpPost("Push")]
        public async Task<IActionResult> Push([FromBody] PushDto pushDto)
        {

        }
        */



        // POST: api/CLI/Pull





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
                    .ThenInclude(b => b.Commits)  // Include commits inside branches
                        .ThenInclude(c => c.Parents)    // Include commit parents
                            .ThenInclude(cp => cp.Parent)   // Include parent commit
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.OwnerId == ownerUser.UserId && r.RepoName == repoName);

            if (repository == null)
                return NotFound(new { Message = "Repository not found" });


            // Private repos need access to the repo
            if (repository.IsPrivate && !repository.RepoAccesses.Any(ra => ra.UserId == userId))
                return NotFound(new { Message = "Repository not found" }); // Repository is hidden, mask unauthorised with not found error



            // Get the clone data
            // (Repo details, Branches, Commits, Trees, File Names/Hashes)
            // Exclude file content

            var treeBuilder = new TreeBuilder(repository.RepoId);

            var branchesData = new List<object>();

            foreach (var branch in repository.Branches)
            {
                var createdByUser = await _janusDbContext.Users
                    .Where(u => u.UserId == branch.CreatedBy)
                    .Select(u => u.Username)
                    .FirstOrDefaultAsync();


                // Get all commits for the branch
                var commits = branch.Commits.Select(commit =>
                {
                    var parentCommitHash = commit.Parents.FirstOrDefault()?.Parent?.CommitHash;

                    // Get the author of the commit
                    string commitAuthorUsername = commit.CreatedBy;
                    string commitAuthorEmail;
                    int commitAuthorId;
                    if (commit.CreatedBy == "Janus")
                    {
                        commitAuthorId = 0;
                        commitAuthorEmail = "Janus";
                    }
                    else
                    {
                        var author = _janusDbContext.Users.FirstOrDefault(u => u.Username == commit.CreatedBy);
                        commitAuthorEmail = author.Email;
                    }

                    // Recreate the tree for the commit
                    TreeNode tree = treeBuilder.RecreateTree(commit.TreeHash);
                    var treeDto = Tree.ConvertTreeNodeToDto(tree);


                    return new
                    {
                        commit.CommitHash,
                        ParentCommitHash = parentCommitHash,
                        AuthorName = commitAuthorUsername,
                        AuthorEmail = commitAuthorEmail,
                        commit.Message,
                        commit.CommittedAt,
                        commit.TreeHash,
                        Tree = treeDto
                    };
                }).ToList();

                branchesData.Add(new
                {
                    BranchName = branch.BranchName,
                    ParentBranch = branch.ParentBranch,
                    SplitFromCommitHash = branch.SplitFromCommitHash,
                    LatestCommitHash = branch.LatestCommitHash,
                    CreatedBy = createdByUser,
                    Commits = commits
                });
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

        public class RepoNameBranch
        {
            public string RepoName { get; set; }
            public string BranchName { get; set; }
        }


        // POST: api/CLI/GetHeadCommitHash
        [HttpPost("RemoteHeadCommit")]
        public async Task<IActionResult> RemoteHeadCommit([FromBody] RepoNameBranch repoNameBranch)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest(ModelState);
            }

            try
            {
                /*
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

                */

                //return Ok(new { CommitHash = commitHash });
                return Ok();

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


    }
}
