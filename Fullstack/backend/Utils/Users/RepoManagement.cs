using backend.DataTransferObjects.CLI;
using backend.Models;
using Microsoft.EntityFrameworkCore;

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
                        AccessLevel = AccessLevel.OWNER
                    };

                    await _janusDbContext.RepoAccess.AddAsync(repoAccess);
                    await _janusDbContext.SaveChangesAsync();




                    // Create data for branch and initial commit
                    var creationDate = DateTime.UtcNow;

                    string initCommitHash = HashHelper.ComputeCommitHash(null, "main", "Janus", "Janus", creationDate, "Initial commit", "");


                    // Create the main branch
                    var mainBranch = new Branch
                    {
                        BranchName = "main",
                        RepoId = repo.RepoId,
                        CreatedBy = 0,
                        CreatedAt = DateTimeOffset.UtcNow,
                        SplitFromCommitHash = null,
                        LatestCommitHash = initCommitHash,
                    };


                    await _janusDbContext.Branches.AddAsync(mainBranch);
                    await _janusDbContext.SaveChangesAsync();



                    // Create the initial commit
                    var initialCommit = new Commit
                    {
                        CommitHash = initCommitHash,
                        BranchId = mainBranch.BranchId,
                        TreeHash = "",
                        CreatedBy = "Janus",
                        Message = "Initial commit",
                        CommittedAt = DateTimeOffset.UtcNow
                    };

                    await _janusDbContext.Commits.AddAsync(initialCommit);
                    await _janusDbContext.SaveChangesAsync();








                    // Commit the transaction
                    await transaction.CommitAsync();

                    return new ReturnObject { Success = true, Message = "Repository initialized successfully" };

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating repo: {ex.Message}");
                    Console.WriteLine($"Error inner: {ex.InnerException?.Message}");

                    // Rollback transaction
                    await transaction.RollbackAsync();
                    return new ReturnObject { Success = false, Message = $"Failed to initialise repository: {ex.Message}" };
                }

            });

        }


    }
}
