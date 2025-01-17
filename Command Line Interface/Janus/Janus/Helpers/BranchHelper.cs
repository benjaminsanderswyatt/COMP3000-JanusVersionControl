using Janus.Models;
using Janus.Plugins;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Janus.Helpers
{

    public class BranchHelper
    {

        public static void UpdateWorkingDirectory(ILogger logger, Paths paths, string branchName)
        {
            string tempWorkingDir = Path.Combine(paths.JanusDir, ".temp");

            try
            {
                Directory.CreateDirectory(tempWorkingDir);

                string branchPath = Path.Combine(paths.HeadsDir, branchName);
                string branchHeadCommit = File.ReadAllText(branchPath);


                // Get the list of files and their content from the commit
                var fileStates = GetAllFilesFromCommitHistory(logger, paths, branchHeadCommit);


                // Recreate the working directory for branch
                RecreateWorkingDirectory(logger, paths, tempWorkingDir, fileStates);


                // Move the files from the temp directory to the working directory
                ReplaceWorkingDirectory(paths.WorkingDir, tempWorkingDir);

                logger.Log($"Updated working directory with files from branch {branchName}.");
            }
            catch (Exception ex)
            {
                logger.Log($"Error updating working directory: {ex.Message}");
            }

            if (Directory.Exists(tempWorkingDir))
            {
                Directory.Delete(tempWorkingDir, true);
            }
        }

        private static void ReplaceWorkingDirectory(string workingDir, string tempWorkingDir)
        {
            // Remove existing files in the working directory
            ClearWorkingDirectory(workingDir);

            // Move files from the temp directory to the working directory
            foreach (var file in Directory.GetFiles(tempWorkingDir, "*", SearchOption.AllDirectories))
            {
                // Get the relative path of the file
                string relativePath = Path.GetRelativePath(tempWorkingDir, file);
                string destPath = Path.Combine(workingDir, relativePath);

                // Move the file
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                File.Move(file, destPath);

            }

        }



        private static void ClearWorkingDirectory(string workingDir)
        {
            try
            {
                // Add all files in the directory recursively (appart from .janus folder)
                var directoryFiles = Directory.GetFiles(workingDir, "*", SearchOption.AllDirectories)
                                              .Where(file => !file.Contains(".janus"));

                foreach (var file in directoryFiles)
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to clear working directory.", ex);
            }
        }

        private static Dictionary<string, string> GetAllFilesFromCommitHistory(ILogger logger, Paths paths, string startingCommit)
        {
            // Create a dictionary to hold all of the files in their states at the point of the commit
            var fileStates = new Dictionary<string, string>();

            // Traverse the commit history to get the files and their states
            string currentCommitHash = startingCommit;
            while (currentCommitHash != null)
            {
                // Load the current commit
                string commitFilePath = Path.Combine(paths.CommitDir, currentCommitHash);
                if (!File.Exists(commitFilePath))
                {
                    throw new FileNotFoundException($"Commit file not found: {commitFilePath}");
                }


                var commitData = JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(commitFilePath));

                foreach (var file in commitData.Files)
                {
                    // Add the latest occurance of the file to the fileState
                    if (!fileStates.ContainsKey(file.Key))
                    {
                        fileStates[file.Key] = file.Value; // Store fileName - Object Hash
                    }
                }

                currentCommitHash = commitData.Parent;

            }

            // Remove deleted files from fileStates
            foreach (var file in fileStates.Keys.ToList())
            {
                if (fileStates[file] == "Deleted")
                {
                    fileStates.Remove(file);
                }
            }


            return fileStates;
        }

        private static void RecreateWorkingDirectory(ILogger logger, Paths paths, string targetDir, Dictionary<string, string> fileStates)
        {
            
            foreach (var file in fileStates)
            {
                try
                {
                    // Get the file content from the object directory
                    string objectFilePath = Path.Combine(paths.ObjectDir, file.Value);
                    if (!File.Exists(objectFilePath))
                    {
                        throw new FileNotFoundException($"Object file not found: {objectFilePath}");
                    }

                    string content = File.ReadAllText(objectFilePath);
                    string filePath = Path.Combine(targetDir, file.Key);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    File.WriteAllText(filePath, content);
                }
                catch (Exception ex)
                {
                    logger.Log($"Failed to recreate file {file.Key}: {ex.Message}");
                    throw;
                }
            }

            logger.Log("Recreated working directory.");
            
        }

        public static bool IsValidBranchName(string branchName)
        {
            if (string.IsNullOrWhiteSpace(branchName))
                return false;


            // ivalid characters: ~ ^ : ? \ * [ ] \x00-\x1F \x7F ..
            var invalidCharsPattern = @"[~^:\?\\\*\[\]\x00-\x1F\x7F]|(\.\.)"; 
            if (Regex.IsMatch(branchName, invalidCharsPattern))
                return false;

            return true;
        }


    }
}
