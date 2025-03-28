using backend.Models;
using backend.Utils;
using Microsoft.EntityFrameworkCore;
using static backend.Utils.TreeBuilder;

namespace backend.Helpers
{
    public class CLIHelper
    {

        private readonly JanusDbContext _janusDbContext;

        public CLIHelper(JanusDbContext janusDbContext)
        {
            _janusDbContext = janusDbContext;
        }


        // Converts a list of commits into their dtos
        public async Task<List<object>> GetCommitDtosAsync(IEnumerable<Commit> commits, TreeBuilder treeBuilder)
        {
            var commitDtos = new List<object>();

            foreach (var commit in commits)
            {
                var parentsCommitHash = commit.Parents
                        .Select(cp => cp.Parent.CommitHash)
                        .ToList();

                // Get the author of the commit
                string commitAuthorUsername = commit.CreatedBy;
                string commitAuthorEmail;

                if (commit.CreatedBy == "Janus")
                {
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

                commitDtos.Add(new
                {
                    commit.CommitHash,
                    parentsCommitHash,
                    AuthorName = commit.CreatedBy,
                    AuthorEmail = commitAuthorEmail,
                    commit.Message,
                    commit.CommittedAt,
                    commit.TreeHash,
                    Tree = treeDto
                });
            }

            return commitDtos;
        }



        // Gets branch data and its whole commit history
        public async Task<object> GetBranchDataAsync(Branch branch, TreeBuilder treeBuilder)
        {
            var createdByUser = await _janusDbContext.Users
                .Where(u => u.UserId == branch.CreatedBy)
                .Select(u => u.Username)
                .FirstOrDefaultAsync();

            var commitDtos = await GetCommitDtosAsync(branch.Commits, treeBuilder);

            return new
            {
                BranchName = branch.BranchName,
                ParentBranch = branch.Parent != null ? branch.Parent.BranchName : null,
                SplitFromCommitHash = branch.SplitFromCommitHash,
                LatestCommitHash = branch.LatestCommitHash,
                CreatedBy = createdByUser,
                Created = branch.CreatedAt,
                Commits = commitDtos
            };
        }




    }
}
