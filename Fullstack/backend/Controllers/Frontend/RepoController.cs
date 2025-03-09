using backend.DataTransferObjects;
using backend.Models;
using backend.Services;
using backend.Utils;
using backend.Utils.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static backend.Utils.TreeBuilder;

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


            var userIdClaim = User.FindFirst("UserId")?.Value;
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
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }






        



        [HttpGet("file/{owner}/{repoName}/{fileHash}")]
        public async Task<IActionResult> GetFileContent(string owner, string repoName, string fileHash)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { Message = "Invalid user" });


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

            string filePath = Path.Combine(fileDir, fileHash);

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { Message = "File not found" });

            // Read file content
            byte[] fileContent = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(fileContent, "application/octet-stream");
        }








        // ------- REPOSITORY LIST -------
        [HttpGet("repository-list")]
        public async Task<IActionResult> GetRepositoryList()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { Message = "Invalid user" });

            // Get all repos owned by the user
            var repositories = await _janusDbContext.Repositories
                .Where(r => r.OwnerId == userId)
                .Include(r => r.Owner)
                .Include(r => r.Branches)
                    .ThenInclude(b => b.Commits)
                .Include(r => r.RepoAccesses)
                    .ThenInclude(ra => ra.User)
                .AsNoTracking()
                .ToListAsync();

            return Ok(repositories.Select(r => new
            {
                Id = r.RepoId,
                Name = r.RepoName,
                Description = r.RepoDescription,
                IsPrivate = r.IsPrivate,
                // Find the latest commit date among all branches (if any)
                LastUpdated = r.Branches
                    .SelectMany(b => b.Commits)
                    .OrderByDescending(c => c.CommittedAt)
                    .FirstOrDefault()?.CommittedAt,

                // Get all collaborators (users with access who are not the owner)
                Colaborators = r.RepoAccesses
                    .Where(ra => ra.UserId != r.OwnerId)
                    .Select(ra => new
                    {
                        Id = ra.UserId,
                        Username = ra.User.Username,
                        AccessLevel = ra.AccessLevel.ToString()
                    }).ToList()
            }));
        }
        // ------- REPO LAYOUT -------

        [HttpGet("{owner}/{repoName}")]
        public async Task<IActionResult> GetRepoData(string owner, string repoName)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { Message = "Invalid user" });

            var repo = await _janusDbContext.Repositories
                .Include(r => r.Owner)
                .Include(r => r.RepoAccesses)
                .Include(r => r.Branches)
                .FirstOrDefaultAsync(r => 
                    r.Owner.Username == owner && 
                    r.RepoName == repoName);

            if (repo == null)
                return NotFound(new { Message = "Repository not found" });

            // Private repos need access to the repo
            if (repo.IsPrivate && !repo.RepoAccesses.Any(ra => ra.UserId == userId))
                return NotFound(new { Message = "Repository not found" }); // Repository is hidden, mask unauthorised with not found error

            return Ok(new
            {
                RepoName = repo.RepoName,
                Description = repo.RepoDescription,
                IsPrivate = repo.IsPrivate,
                Owner = repo.Owner.Username,
                Branches = repo.Branches.Select(b => b.BranchName).ToArray(),
                CreatedAt = repo.CreatedAt,
            });

        }


        // ------- REPO PAGE -------
        [HttpGet("{owner}/{repoName}/{branch}")]
        public async Task<IActionResult> GetBranchData(string owner, string repoName, string branch)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { Message = "Invalid user" });

            var repo = await _janusDbContext.Repositories
                .Include(r => r.Owner)
                .Include(r => r.RepoAccesses)
                .Include(r => r.Branches.Where(b => b.BranchName == branch))
                    .ThenInclude(b => b.Commits)
                        .ThenInclude(c => c.Parents)
                            .ThenInclude(cp => cp.Parent)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.Owner.Username == owner &&
                    r.RepoName == repoName);

            if (repo == null)
                return NotFound(new { Message = "Repository not found" });

            // Private repos need access to the repo
            if (repo.IsPrivate && !repo.RepoAccesses.Any(ra => ra.UserId == userId))
                return NotFound(new { Message = "Repository not found" }); // Repository is hidden, mask unauthorised with not found error





            var targetBranch = repo.Branches.FirstOrDefault(b => b.BranchName == branch);
            if (targetBranch == null)
                return NotFound(new { Message = "Branch not found" });

            var latestCommit = targetBranch.Commits
                .FirstOrDefault(c => c.CommitHash == targetBranch.LatestCommitHash);
            if (latestCommit == null)
                return NotFound(new { Message = "Latest commit not found" });



            // Get the author of the latest commit
            string commitAuthorUsername;
            int commitAuthorId = latestCommit.CreatedBy;
            if (commitAuthorId == 0)
            {
                commitAuthorUsername = "Janus";
            }
            else
            {
                var author = _janusDbContext.Users.FirstOrDefault(u => u.UserId == commitAuthorId);
                commitAuthorUsername = author.Username;
            }
            



            // Rebuild the latest commits tree
            var treeBuilder = new TreeBuilder(repo.RepoId);
            TreeNode treeRoot = treeBuilder.RecreateTree(latestCommit.TreeHash);
            var treeDto = Tree.ConvertTreeNodeToDto(treeRoot);


            // Load the README.md file
            var readmeNode = treeRoot.Children.FirstOrDefault(child => child.Name.Equals("README.md", StringComparison.OrdinalIgnoreCase));
            
            string? readmeContent = null;
            if (readmeNode != null)
            {
                string fileDir = Path.Combine(
                    Environment.GetEnvironmentVariable("FILE_STORAGE_PATH"),
                    repo.RepoId.ToString()
                );
                string filePath = Path.Combine(fileDir, readmeNode.Hash);

                if (System.IO.File.Exists(filePath))
                {
                    byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                    readmeContent = Encoding.UTF8.GetString(fileBytes);
                }
            }
            
            




            return Ok(new
            {
                LatestCommit = new
                {
                    UserId = commitAuthorId,
                    UserName = commitAuthorUsername,
                    Message = latestCommit.Message,
                    Hash = latestCommit.CommitHash,
                    Date = latestCommit.CommittedAt,
                },
                Readme = readmeContent,
                Tree = treeDto
            });

        }

        





    }
}
