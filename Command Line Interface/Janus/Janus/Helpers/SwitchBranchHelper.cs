using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Janus.Helpers
{

    public class SwitchBranchHelper
    {
        /*
        public static void SwitchBranch(ILogger logger, Paths paths, string currentBranch, string branchName)
        {

            // Get file states for current and target branch

            // Get the current branch
            string headCommitHash = CommandHelper.GetCurrentHEAD(paths);
            var currentTree = TreeHelper.GetTreeFromCommitHash(paths, headCommitHash);
            var currentFiles = TreeHelper.GetAllFilePathsWithHashesRecursive(currentTree, paths.WorkingDir);



            // Get the target branch
            string targetCommit = File.ReadAllText(Path.Combine(paths.HeadsDir, branchName));
            var targetTree = TreeHelper.GetTreeFromCommitHash(paths, targetCommit);
            var targetFiles = TreeHelper.GetAllFilePathsWithHashesRecursive(targetTree, paths.WorkingDir);


            // Work out files to add, update or delete
            // Add -> file exists in target but not in current working dir
            // Update -> file exists in target and in current working dir but hashes are different
            // Delete -> file exists in current working dir but not in target
            var filesToAddOrUpdate = targetFiles.Where(file =>
                                        !currentFiles.ContainsKey(file.Key)
                                        || currentFiles[file.Key] != file.Value);

            var filesToDelete = currentFiles.Where(file => !targetFiles.ContainsKey(file.Key));





            // Update the working dir
            // Add or update files
            foreach (var file in filesToAddOrUpdate)
            {
                //var filePath = file.Key;
                var filePath = Path.Combine(paths.WorkingDir, file.Key);
                var fileContents = File.ReadAllBytes(Path.Combine(paths.ObjectDir, file.Value));

                File.WriteAllBytes(filePath, fileContents);
            }

            // Delete files
            foreach (var file in filesToDelete)
            {
                var filePath = Path.Combine(paths.WorkingDir, file.Key);

                File.Delete(filePath);
            }




            // Update HEAD to point to the target branch
            File.WriteAllText(paths.HEAD, targetCommit);



            // Save index and head of current branch
            File.Copy(paths.Index, Path.Combine(paths.BranchesDir, currentBranch, "index"), overwrite: true);



            // Update index and head to new branch
            File.Copy(Path.Combine(paths.BranchesDir, branchName, "index"), paths.Index, overwrite: true);

            File.WriteAllText(paths.HEAD, $"ref: heads/{branchName}");



            logger.Log($"Successfully switched to branch '{branchName}'.");
        }
        */
    


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
                string relativePath = Path.GetRelativePath(paths.WorkingDir, filePath);
                string targetHash = GetHashFromTree(targetTree, relativePath); // Helper to get the hash of a file from the tree
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














        public static string GetHashFromTree(TreeNode tree, string relativePath)
        {
            string[] pathParts = PathHelper.PathSplitter(relativePath);
            TreeNode currentNode = tree;

            foreach (var part in pathParts)
            {
                currentNode = currentNode.Children.FirstOrDefault(c => c.Name == part);
                if (currentNode == null) break;
            }

            return currentNode?.Hash;
        }

    }




}