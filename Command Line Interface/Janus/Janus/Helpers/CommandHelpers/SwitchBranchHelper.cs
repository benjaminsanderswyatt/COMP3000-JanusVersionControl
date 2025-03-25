using Janus.Plugins;
using Janus.Utils;

namespace Janus.Helpers.CommandHelpers
{

    public class SwitchBranchHelper
    {
        public static void SwitchBranch(ILogger logger, Paths paths, string currentBranch, string branchName)
        {
            var treeBuilder = new TreeBuilder(paths);

            var currentTree = Tree.GetHeadTree(logger, paths);
            var targetTree = Tree.GetHeadTree(logger, paths, branchName);
            var comparisonResult = Tree.CompareTrees(currentTree, targetTree);


            // Add or update files
            foreach (var relativePath in comparisonResult.AddedOrUntracked.Concat(comparisonResult.ModifiedOrNotStaged))
            {
                string targetHash = Tree.GetHashFromTree(targetTree, relativePath);
                string objectFilePath = Path.Combine(paths.ObjectDir, targetHash);

                // Check the object file exists
                if (File.Exists(objectFilePath))
                {
                    string fullPath = Path.Combine(paths.WorkingDir, relativePath);
                    string directory = Path.GetDirectoryName(fullPath);

                    // Create parent directories if needed
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    File.WriteAllBytes(fullPath, File.ReadAllBytes(objectFilePath));
                }
                else
                {
                    logger.Log($"Warning: Missing object file for {relativePath}");
                }
            }

            // Delete files
            foreach (var relativePath in comparisonResult.Deleted)
            {
                string fullPath = Path.Combine(paths.WorkingDir, relativePath);

                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }

            // Cleanup empty dirs
            var targetDirectories = GetAllDirs(targetTree);
            RemoveOrphanedDirs(paths.WorkingDir, targetDirectories);

            // Update branch references
            BranchHelper.SetCurrentHEAD(paths, branchName);
            UpdateBranchIndexes(logger, paths, currentBranch, branchName);

            logger.Log($"Switched to branch '{branchName}'");
        }


        // Collect all directory paths from a tree
        private static HashSet<string> GetAllDirs(TreeNode tree)
        {
            var directories = new HashSet<string>();
            CollectDirectories(tree, "", directories);
            return directories;
        }

        private static void CollectDirectories(TreeNode node, string currentPath, HashSet<string> directories)
        {
            if (node.Hash != null) return; // Skip files

            if (!string.IsNullOrEmpty(currentPath))
                directories.Add(currentPath);

            foreach (var child in node.Children)
            {
                string childPath = string.IsNullOrEmpty(currentPath)
                    ? child.Name
                    : Path.Combine(currentPath, child.Name);
                CollectDirectories(child, childPath, directories);
            }
        }


        // Remove directories not present in target branch
        private static void RemoveOrphanedDirs(string workingDir, HashSet<string> targetDirectories)
        {
            var workingDirs = Directory.GetDirectories(workingDir, "*", SearchOption.AllDirectories)
                .Select(d => Path.GetRelativePath(workingDir, d))
                .OrderByDescending(d => d.Length);

            foreach (var relativePath in workingDirs)
            {
                string fullPath = Path.Combine(workingDir, relativePath);

                if (!targetDirectories.Contains(relativePath) && IsDirectoryEmpty(fullPath))
                    Directory.Delete(fullPath, recursive: false);
            }
        }

        private static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }


        private static void UpdateBranchIndexes(ILogger logger, Paths paths, string currentBranch, string targetBranch)
        {
            // Save current branch's index state
            var currentIndexPath = Path.Combine(paths.BranchesDir, currentBranch, "index");
            File.Copy(paths.Index, currentIndexPath, overwrite: true);

            // Load target branch's index
            var targetIndexPath = Path.Combine(paths.BranchesDir, targetBranch, "index");
            if (File.Exists(targetIndexPath))
            {
                File.Copy(targetIndexPath, paths.Index, overwrite: true);
            }
            else
            {
                File.WriteAllText(paths.Index, string.Empty);
                logger.Log($"Initialized empty index for branch '{targetBranch}'");
            }
        }







    }

}