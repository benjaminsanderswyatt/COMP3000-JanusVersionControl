using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace backend.Utils.Users
{
    public class RepoManagement
    {
        private readonly JanusDbContext _janusDbContext;
        private readonly UserManagement _userManagement;

        public RepoManagement(JanusDbContext janusDbContext, UserManagement userManagement)
        {
            _janusDbContext = janusDbContext;
            _userManagement = userManagement;
        }


        // Check if repo with the same name exists for user
        public async Task<bool> RepoWithNameExistsAsync(int ownerId, string repoName)
        {
            return await _janusDbContext.Repositories.AnyAsync(r => r.OwnerId == ownerId && r.RepoName == repoName);
        }



        // Get repo by Id
        public async Task<Repository?> GetRepositoryAsync(int repoId)
        {
            return await _janusDbContext.Repositories
                                        .Include(r => r.Owner)
                                        .Include(r => r.RepoAccesses)
                                        .FirstOrDefaultAsync(r => r.RepoId == repoId);
        }


        // Get all repos of user
        public async Task<List<Repository>> GetAllReposOfUserAsync(int userId)
        {
            return await _janusDbContext.Repositories.Where(r => r.OwnerId == userId).ToListAsync();
        }



        // Get repo by name
        public async Task<Repository?> GetRepositoryByNameAsync(int ownerId, string repoName)
        {
            return await _janusDbContext.Repositories.FirstOrDefaultAsync(r => r.OwnerId == ownerId && r.RepoName == repoName);
        }


        // Create a repo
        public async Task<ReturnObject> CreateRepoAsync(Repository repo)
        {
            var existingRepo = await RepoWithNameExistsAsync(repo.OwnerId, repo.RepoName);
            if (existingRepo)
                return new ReturnObject { Success = false, Message = $"Repository '{repo.RepoName}' already exists" };

            await _janusDbContext.Repositories.AddAsync(repo);
            await _janusDbContext.SaveChangesAsync();

            return new ReturnObject { Success = true, Message = "Repository created successfully" };
        }


        // Delete repo
        public async Task<ReturnObject> DeleteRepoAsync(int repoId)
        {
            var repository = await _janusDbContext.Repositories.FindAsync(repoId);
            if (repository == null)
                return new ReturnObject { Success = false, Message = "Failed to delete repository" };

            _janusDbContext.Repositories.Remove(repository);
            await _janusDbContext.SaveChangesAsync();

            return new ReturnObject { Success = true, Message = "Repository deleted successfully" };
        }


        // Atomic repo init
        public async Task<ReturnObject> InitRepoAsync(int ownerId, string repoName, string repoDescription, bool isPrivate)
        {
            // Check if repo with same name exists
            if (await RepoWithNameExistsAsync(ownerId, repoName))
            {
                return new ReturnObject { Success = false, Message = $"Repository '{repoName}' already exists" };
            }

            // Fetch the user's details (name and email) from the database
            var user = await _janusDbContext.Users
                .Where(u => u.UserId == ownerId)
                .Select(u => new { u.Username, u.Email })
                .FirstOrDefaultAsync();

            if (user == null)
                return new ReturnObject { Success = false, Message = "User not found" };

            string authorName = user.Username;
            string authorEmail = user.Email;



            var executionStrategy = _janusDbContext.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                // Start transaction
                using var transaction = await _janusDbContext.Database.BeginTransactionAsync();


                try
                {
                    // Create the repository
                    var repo = new Repository
                    {
                        OwnerId = ownerId,
                        RepoName = repoName,
                        RepoDescription = repoDescription,
                        IsPrivate = isPrivate,
                    };

                    await _janusDbContext.Repositories.AddAsync(repo);
                    await _janusDbContext.SaveChangesAsync();



                    // Create repo access for owner
                    var repoAccess = new RepoAccess
                    {
                        RepoId = repo.RepoId,
                        UserId = ownerId,
                        AccessLevel = AccessLevel.ADMIN
                    };

                    await _janusDbContext.RepoAccess.AddAsync(repoAccess);
                    await _janusDbContext.SaveChangesAsync();



                    // Create initial commit
                    string initialCommitMessage = "Initial commit";
                    string emptyTreeHash = "";
                    string initCommitHash = ComputeCommitHash(emptyTreeHash, initialCommitMessage);

                    var commit = new Commit
                    {
                        CommitHash = initCommitHash,
                        TreeHash = emptyTreeHash,
                        BranchName = "main",
                        AuthorName = authorName,
                        AuthorEmail = authorEmail,
                        Message = initialCommitMessage
                    };

                    await _janusDbContext.Commits.AddAsync(commit);
                    await _janusDbContext.SaveChangesAsync();



                    // Create main branch
                    var branch = new Branch
                    {
                        BranchName = "main",
                        RepoId = repo.RepoId,
                        LatestCommitHash = initCommitHash,
                    };

                    await _janusDbContext.Branches.AddAsync(branch);
                    await _janusDbContext.SaveChangesAsync();






                    // Commit the transaction
                    await transaction.CommitAsync();

                    return new ReturnObject { Success = true, Message = "Repository initialized successfully" };

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating repo: {ex.Message}");

                    // Rollback transaction
                    await transaction.RollbackAsync();
                    return new ReturnObject { Success = false, Message = $"Failed to initialise repository: {ex.Message}" };
                }

            });

        }

        private static string ComputeCommitHash(string treeHash, string commitMessage)
        {
            byte[] combined = Encoding.UTF8.GetBytes(treeHash + commitMessage);

            return ComputeHash(combined);
        }

        private static string ComputeHash(byte[] contentBytes)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(contentBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

    }
}
