using Janus.Models;
using Janus.Plugins;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Janus.Helpers
{

    public class SwitchBranchHelper
    {
        
        public static void SwitchBranch(ILogger logger, Paths paths, string branchName)
        {
            string backupWorkingDir = Path.Combine(paths.JanusDir, ".backup");
            string backupIndex = Path.Combine(paths.JanusDir, "index.backup");
            string backupHEAD = Path.Combine(paths.JanusDir, "HEAD.backup");

            try
            {
                // Backup the index and HEAD
                BackupIndexAndHead(paths, backupIndex, backupHEAD);


                // Load branch data
                var files = LoadBranchData(paths, branchName);


                // Backup and replace working directory
                BackupAndReplaceWorkingDirectory(logger, paths, files, backupWorkingDir);


                // Update branch metadata
                UpdateBranchMetadata(logger, paths, branchName);


                logger.Log($"Successfully switched to branch '{branchName}'.");
            }
            catch (Exception ex)
            {
                logger.Log($"Error switching branch: {ex.Message}");

                // Rollback changes on failure
                RollbackToBackup(logger, paths, backupWorkingDir, backupIndex, backupHEAD);
            }


            // Cleanup temporary backups
            CleanupBackups(backupWorkingDir, backupIndex, backupHEAD);
        }


        private static void BackupIndexAndHead(Paths paths, string backupIndex, string backupHEAD)
        {
            try
            {
                File.Copy(paths.Index, backupIndex, overwrite: true);
                File.Copy(paths.HEAD, backupHEAD, overwrite: true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to backup index and HEAD.", ex);
            }
        }


        private static Dictionary<string, string> LoadBranchData(Paths paths, string branchName)
        {
            try
            {
                // Locate branch commit
                string branchPath = Path.Combine(paths.HeadsDir, branchName);
                string branchHeadCommit = File.ReadAllText(branchPath);


                // Retrieve file states from the branch
                var tree = TreeHelper.GetTreeFromCommitHash(paths, branchHeadCommit);
                var files = TreeHelper.GetAllFilePathsWithHashesRecursive(tree, paths.ObjectDir);

                return files;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load branch data for {branchName}.", ex);
            }
        }


        private static void BackupAndReplaceWorkingDirectory(ILogger logger, Paths paths, Dictionary<string, string> files, string backupWorkingDir)
        {
            try
            {
                // Backup working directory
                Directory.CreateDirectory(backupWorkingDir);
                foreach (var file in GetFilesHelper.GetAllFilesInDir(paths, paths.WorkingDir))
                {
                    string relativePath = Path.GetRelativePath(paths.WorkingDir, file);
                    string destPath = Path.Combine(backupWorkingDir, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.Copy(file, destPath, overwrite: true);
                }

                // Recreate working directory with branch files
                RecreateWorkingDirectory(logger, paths, backupWorkingDir, files);

                // Replace working directory
                ReplaceWorkingDirectory(paths, backupWorkingDir);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to replace the working directory.", ex);
            }
        }


        private static void RecreateWorkingDirectory(ILogger logger, Paths paths, string targetWorkingDir, Dictionary<string, string> fileStates)
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
                    string filePath = Path.Combine(targetWorkingDir, file.Key);

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


        private static void UpdateBranchMetadata(ILogger logger, Paths paths, string branchName)
        {
            // Save the current branch index
            string previousBranch = CommandHelper.GetCurrentBranchName(paths);
            File.Copy(paths.Index, Path.Combine(paths.BranchesDir, previousBranch, "index"), overwrite: true);

            // Load the index for the new branch
            File.Copy(Path.Combine(paths.BranchesDir, branchName, "index"), paths.Index, overwrite: true);

            // Update HEAD to point to the new branch
            File.WriteAllText(paths.HEAD, $"ref: heads/{branchName}");
        }


        private static void RollbackToBackup(ILogger logger, Paths paths, string backupWorkingDir, string backupIndex, string backupHEAD)
        {
            logger.Log("Rolling back to previous state...");

            if (Directory.Exists(backupWorkingDir))
            {
                ClearWorkingDirectory(paths);
                foreach (var file in Directory.GetFiles(backupWorkingDir, "*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(backupWorkingDir, file);
                    string destPath = Path.Combine(paths.WorkingDir, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.Copy(file, destPath, overwrite: true);
                }
            }

            if (File.Exists(backupIndex))
                File.Copy(backupIndex, paths.Index, overwrite: true);

            if (File.Exists(backupHEAD))
                File.Copy(backupHEAD, paths.HEAD, overwrite: true);

            logger.Log("Rollback completed.");
        }


        private static void CleanupBackups(string backupWorkingDir, string backupIndex, string backupHEAD)
        {
            if (Directory.Exists(backupWorkingDir))
            {
                Directory.Delete(backupWorkingDir, true);
            }

            if (File.Exists(backupIndex))
            {
                File.Delete(backupIndex);
            }

            if (File.Exists(backupHEAD))
            {
                File.Delete(backupHEAD);
            }
        }


        private static void ReplaceWorkingDirectory(Paths paths, string tempWorkingDir)
        {
            // Remove existing files in the working directory
            ClearWorkingDirectory(paths);

            // Move files from the temp directory to the working directory
            foreach (var file in Directory.GetFiles(tempWorkingDir, "*", SearchOption.AllDirectories))
            {
                // Get the relative path of the file
                string relativePath = Path.GetRelativePath(tempWorkingDir, file);
                string destPath = Path.Combine(paths.WorkingDir, relativePath);

                // Move the file
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                File.Move(file, destPath);

            }

        }


        private static void ClearWorkingDirectory(Paths paths)
        {
            try
            {
                // Add all files in the directory recursively
                var directoryFiles = GetFilesHelper.GetAllFilesInDir(paths, paths.WorkingDir);

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




    }
}
