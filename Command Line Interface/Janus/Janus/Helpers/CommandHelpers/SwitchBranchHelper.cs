using Janus.Plugins;
using Janus.Utils;

namespace Janus.Helpers.CommandHelpers
{

    public class SwitchBranchHelper
    {
        /*
        public static void SwitchBranch(ILogger logger, Paths paths, string currentBranch, string branchName)
        {
            // Initialise TreeBuilder for operations
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

                    // Ensure the directory exists
                    string directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

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

        */





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