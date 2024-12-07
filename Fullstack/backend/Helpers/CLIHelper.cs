using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Helpers
{
    public class CLIHelper
    {

        private readonly JanusDbContext _janusDbContext;

        public CLIHelper(JanusDbContext janusDbContext)
        {
            _janusDbContext = janusDbContext;
        }

        public async Task<int?> GetParentCommitIdAsync(string parentCommitHash)
        {
            if (string.IsNullOrWhiteSpace(parentCommitHash))
                return null;

            // Find the parent commit
            var parentCommit = await _janusDbContext.Commits
                .FirstOrDefaultAsync(c => c.CommitHash == parentCommitHash);

            // Return the ParentCommitId or null if not found
            return parentCommit?.CommitId;
        }
    }
}
