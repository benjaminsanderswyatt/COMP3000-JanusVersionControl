using backend.DataTransferObjects;
using backend.Models;
using backend.Utils.Users;

namespace backend.Services
{
    public class RepoService
    {
        private readonly RepoManagement _repoManagement;

        public RepoService(RepoManagement repoManagement)
        {
            _repoManagement = repoManagement;
        }

        public async Task<ReturnObject> CreateRepoAsync(int userId, RepositoryDto newRepo)
        {
            var newRepository = new Repository
            {
                OwnerId = userId,
                RepoName = newRepo.RepoName,
                IsPrivate = newRepo.IsPrivate
            };

            return await _repoManagement.CreateRepoAsync(newRepository);
        }

    }
}
