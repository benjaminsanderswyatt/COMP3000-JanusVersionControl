using Janus.Models;
using Janus.Plugins;
using System.Text.Json;

namespace Janus.Helpers
{

    public class BranchHelper
    {



        public static void UpdateWorkingDirectory(ILogger logger, Paths paths, string branchName)
        {
            // Get the latest commit hash of the branch
            string branchPath = Path.Combine(paths.HeadsDir, branchName);
            string branchHeadCommit = File.ReadAllText(branchPath);



            // Get the list of files and their content from the commit
            Dictionary<string, string> fileStates = GetAllFilesFromCommitHistory(logger, paths, branchHeadCommit);



            // Clear the working directory
            ClearWorkingDirectory(paths);



            // Recreate the files for the branch in the working directory
            RecreateWorkingDirectory(logger, paths, fileStates);



            logger.Log($"Updated working directory with files from branch {branchName}.");

        }

        public static void ClearWorkingDirectory(Paths paths)
        {
            // Add all files in the directory recursively
            var directoryFiles = Directory.GetFiles(paths.WorkingDir, "*", SearchOption.AllDirectories)
                                          .Where(file => !file.Contains(".janus"));

            foreach (var file in directoryFiles)
            {
                File.Delete(file);
            }
        }

        public static Dictionary<string, string> GetAllFilesFromCommitHistory(ILogger logger, Paths paths, string startingCommit)
        {
            // Create a dictionary to hold all of the files in their states at the point of the commit
            Dictionary<string, string> fileStates = new Dictionary<string, string>();

            // Traverse the commit history to get the files and their states
            string currentCommitHash = startingCommit;
            while (currentCommitHash != null)
            {
                // Load the current commit
                string commitFilePath = Path.Combine(paths.CommitDir, currentCommitHash);
                string commitData = File.ReadAllText(commitFilePath);
                var commit = JsonSerializer.Deserialize<CommitMetadata>(commitData);

                foreach (var file in commit.Files)
                {
                    // If the file isnt in the filestate then add it
                    if (!fileStates.ContainsKey(file.Key))
                    {
                        fileStates[file.Key] = file.Value; // Store fileName - Object Hash
                    }
                }

                currentCommitHash = commit.Parent;

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

        public static void RecreateWorkingDirectory(ILogger logger, Paths paths, Dictionary<string, string> fileStates)
        {
            foreach (var file in fileStates)
            {
                // Get the file content from the object directory
                string objectFilePath = Path.Combine(paths.ObjectDir, file.Value);
                string content = File.ReadAllText(objectFilePath);
                string filePath = Path.Combine(paths.WorkingDir, file.Key);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllText(filePath, content);
            }

            logger.Log("Recreated working directory.");
        }



    }
}
