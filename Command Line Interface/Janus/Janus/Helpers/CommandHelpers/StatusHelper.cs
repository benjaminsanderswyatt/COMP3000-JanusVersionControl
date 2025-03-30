using Janus.Plugins;
using Janus.Utils;

namespace Janus.Helpers.CommandHelpers
{
    public class StatusHelper
    {
        // stagedTree and headTree are optional
        public static bool HasAnythingBeenStagedForCommit(ILogger logger, Paths paths, TreeNode stagedTree = null, TreeNode headTree = null)
        {
            var comparisonResult = GetAddedModifiedDeleted(logger, paths, stagedTree, headTree);

            if (comparisonResult.AddedOrUntracked.Any() || comparisonResult.ModifiedOrNotStaged.Any() || comparisonResult.Deleted.Any())
            {
                return true;
            }

            return false;
        }


        public static bool AreThereUncommittedChanges(ILogger logger, Paths paths, TreeNode stagedTree = null, TreeNode workingTree = null)
        {
            var addedModifiedDeleted = GetAddedModifiedDeleted(logger, paths, stagedTree);

            if (addedModifiedDeleted.AddedOrUntracked.Any() || addedModifiedDeleted.ModifiedOrNotStaged.Any() || addedModifiedDeleted.Deleted.Any())
            {
                return true;
            }


            var notStagedUntracked = GetNotStagedUntracked(paths, stagedTree, workingTree);

            if (notStagedUntracked.ModifiedOrNotStaged.Any() || notStagedUntracked.AddedOrUntracked.Any())
            {
                return true;
            }

            return false;
        }







        // Compare head to tree
        public static TreeComparisonResult GetAddedModifiedDeleted(ILogger logger, Paths paths, TreeNode stagedTree = null, TreeNode headTree = null)
        {
            // Load staged tree if not provided
            if (stagedTree == null)
            {
                stagedTree = Tree.GetStagedTree(paths);
            }


            // Load head tree if not provided
            if (headTree == null)
            {
                headTree = Tree.GetHeadTree(logger, paths);
            }


            // Compare head to staged tree (differances are changes to be committed)
            var comparisonResult = Tree.CompareTrees(headTree, stagedTree);

            return comparisonResult;
        }

        // Compare staged to working
        public static TreeComparisonResult GetNotStagedUntracked(Paths paths, TreeNode stagedTree = null, TreeNode workingTree = null)
        {
            // Load staged tree if not provided
            if (stagedTree == null)
            {
                stagedTree = Tree.GetStagedTree(paths);
            }

            // Load working tree if not provided
            if (workingTree == null)
            {
                workingTree = Tree.GetWorkingTree(paths);
            }


            // Compare staged tree to working tree (differances are not staged for commit or untracked)
            var comparisonResult = Tree.CompareTrees(stagedTree, workingTree);

            return comparisonResult;
        }




        public static void DisplayStatus(ILogger logger, List<string> list, ConsoleColor? colour = null, string addon = null)
        {
            if (colour.HasValue)
            {
                Console.ForegroundColor = colour.Value;
            }

            foreach (var file in list)
            {
                logger.Log($"    {file} {addon}");
            }

            Console.ResetColor();
        }







        public class SyncStatus
        {
            public bool FoundLocalInRemote { get; set; }
            public bool FoundRemoteInLocal { get; set; }
            public int CommitsBehind { get; set; }
            public int CommitsAhead { get; set; }
        }

        public static SyncStatus CheckSyncStatus(Paths paths, string remoteHead, string localHead)
        {
            var status = new SyncStatus();

            // Traverse remote history to see if it contains the local head
            string currentHash = remoteHead;
            while (!string.IsNullOrEmpty(currentHash))
            {
                if (currentHash == localHead)
                {
                    status.FoundLocalInRemote = true;
                    break;
                }

                var commit = RepoHelper.LoadCommit(paths, currentHash);
                if (commit == null || commit.Parents == null || !commit.Parents.Any())
                    break;

                currentHash = commit.Parents.First();
                status.CommitsBehind++;
            }

            // Traverse local history to see if it contains the remote head
            currentHash = localHead;
            while (!string.IsNullOrEmpty(currentHash))
            {
                if (currentHash == remoteHead)
                {
                    status.FoundRemoteInLocal = true;
                    break;
                }

                var commit = RepoHelper.LoadCommit(paths, currentHash);
                if (commit == null || commit.Parents == null || !commit.Parents.Any())
                    break;

                currentHash = commit.Parents.First();
                status.CommitsAhead++;
            }

            return status;
        }






















    }
}
