using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using System.Text.Json;

namespace Janus.Helpers
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

    }
}
