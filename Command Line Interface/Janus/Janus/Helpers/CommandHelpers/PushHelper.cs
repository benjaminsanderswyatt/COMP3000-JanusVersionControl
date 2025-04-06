using Janus.DataTransferObjects;
using Janus.Plugins;
using Janus.Utils;

namespace Janus.Helpers.CommandHelpers
{
    public class PushHelper
    {


        public static List<CommitDto> GetLocalCommitsBetween(ILogger logger, Paths paths, string remoteHead, string localHead)
        {
            var commits = new List<CommitDto>();
            string currentHash = localHead;


            while (!string.IsNullOrEmpty(currentHash) && currentHash != remoteHead)
            {
                var commit = RepoHelper.LoadCommit(paths, currentHash);
                if (commit == null)
                    break;

                var tree = Diff.GetTreeFromCommit(logger, paths, currentHash);

                TreeDto treeDto = TreeBuilder.ConvertTreeNodeToTreeDto(tree);

                var commitDto = new CommitDto
                {
                    CommitHash = commit.Commit,
                    ParentsCommitHash = commit.Parents,
                    AuthorName = commit.AuthorName,
                    AuthorEmail = commit.AuthorEmail,
                    Message = commit.Message,
                    CommittedAt = commit.Date,
                    TreeHash = treeDto.Hash,
                    Tree = treeDto
                };

                commits.Add(commitDto);

                // Move to the first parent commit
                currentHash = commit.Parents.FirstOrDefault();
            }

            commits.Reverse();

            return commits;
        }


    }
}
