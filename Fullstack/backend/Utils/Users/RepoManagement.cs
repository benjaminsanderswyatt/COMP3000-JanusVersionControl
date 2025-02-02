using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace backend.Utils.Users
{
    public class RepoManagement
    {
        private readonly JanusDbContext _janusDbContext;

        public RepoManagement(JanusDbContext janusDbContext)
        {
            _janusDbContext = janusDbContext;
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
                return new ReturnObject { Success = false, Message = "Failed to delete repository"};

            _janusDbContext.Repositories.Remove(repository);
            await _janusDbContext.SaveChangesAsync();

            return new ReturnObject { Success = true, Message = "Repository deleted successfully" };
        }




    }
}
