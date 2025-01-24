using Janus.Models;
using Janus.Plugins;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Janus.Helpers
{

    public class SwitchBranchHelper
    {
        
        public static void SwitchBranch(ILogger logger, Paths paths,string currentBranch, string branchName)
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
                var fileContents = File.ReadAllText(Path.Combine(paths.ObjectDir, file.Value));

                File.WriteAllText(filePath, fileContents);
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

    }

}

