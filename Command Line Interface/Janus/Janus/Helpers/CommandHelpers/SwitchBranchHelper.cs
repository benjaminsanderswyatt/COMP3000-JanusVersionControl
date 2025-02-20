using Janus.Plugins;
using Janus.Utils;

namespace Janus.Helpers.CommandHelpers
{

    public class SwitchBranchHelper
    {
        public static void SwitchBranch(ILogger logger, Paths paths, string currentBranch, string branchName)
        {
            // Initialize TreeBuilder for operations
            var treeBuilder = new TreeBuilder(paths);

            // Get the current branch
            var currentTree = Tree.GetHeadTree(logger, paths);

            // Get the target branch tree
            var targetTree = Tree.GetHeadTree(logger, paths, branchName);

            // Compare the current and target trees to determine actions
            var comparisonResult = Tree.CompareTrees(currentTree, targetTree);


            // Perform actions based on the comparison result
            // Add or update files
            foreach (var filePath in comparisonResult.AddedOrUntracked.Concat(comparisonResult.ModifiedOrNotStaged))
            {
                string relativePath = filePath;

                string targetHash = Tree.GetHashFromTree(targetTree, relativePath);
                string objectFilePath = Path.Combine(paths.ObjectDir, targetHash);


                // Check the object file exists
                if (File.Exists(objectFilePath))
                {
                    var fileContents = File.ReadAllBytes(objectFilePath);
                    File.WriteAllBytes(filePath, fileContents);
                }
                else
                {
                    logger.Log($"Warning: Object file for {relativePath} not found in {paths.ObjectDir}");
                }
            }

            // Delete files
            foreach (var filePath in comparisonResult.Deleted)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }



            // Update HEAD to point to the target branch
            BranchHelper.SetCurrentHEAD(paths, branchName);

            // Save the current branch's index and HEAD
            var currentBranchIndexPath = Path.Combine(paths.BranchesDir, currentBranch, "index");
            File.Copy(paths.Index, currentBranchIndexPath, overwrite: true);

            // Update index and HEAD for the new branch
            var targetBranchIndexPath = Path.Combine(paths.BranchesDir, branchName, "index");
            if (File.Exists(targetBranchIndexPath))
            {
                File.Copy(targetBranchIndexPath, paths.Index, overwrite: true);
            }
            else
            {
                logger.Log($"Warning: Index file for branch '{branchName}' not found. Using an empty index.");
                File.WriteAllText(paths.Index, string.Empty); // Clear index if it doesn't exist for the target branch
            }

            logger.Log($"Successfully switched to branch '{branchName}'.");
        }



    }




}