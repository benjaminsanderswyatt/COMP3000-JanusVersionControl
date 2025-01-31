using System.Text.RegularExpressions;

namespace Janus.CommandHelpers
{

    public class DeleteBranchHelper
    {


        public static bool IsValidBranchName(string branchName)
        {
            if (string.IsNullOrWhiteSpace(branchName))
                return false;


            // ivalid characters: ~ ^ : ? / \ * [ ] \x00-\x1F \x7F ..
            var invalidCharsPattern = @"[~^:\?\\\*/\[\]\x00-\x1F\x7F]|(\.\.)";
            if (Regex.IsMatch(branchName, invalidCharsPattern))
                return false;

            return true;
        }



        /*
        public static void DeleteBranchCommitAndFiles(ILogger logger, Paths paths, string branchName)
        {
            string tempDeletionDir = Path.Combine(paths.JanusDir, ".temp_deletion");

            try
            {
                Directory.CreateDirectory(tempDeletionDir);

                // Get all the commits in the commit directory
                var allCommits = GetAllCommits(logger, paths);

                // Find commits belonging to the branch
                var branchCommits = allCommits.Where(c => c.Branch == branchName).ToList();


                // Find the files exclusive to branch
                var branchFiles = branchCommits.SelectMany(c => c.Files.Keys).ToHashSet();

                var otherBranchFiles = allCommits.Where(c => c.Branch != branchName)
                                                 .SelectMany(c => c.Files.Keys)
                                                 .ToHashSet();

                // Files exclusive to the branch
                var filesToDelete = branchFiles.Except(otherBranchFiles).ToList();


                // Backup the files to be deleted
                BackupDeletion(paths, tempDeletionDir, filesToDelete, branchCommits);
                


                // Delete the files that are unique to the branch
                foreach (var file in filesToDelete)
                {
                    string objectFilePath = Path.Combine(paths.ObjectDir, file);
                    if (File.Exists(objectFilePath))
                    {
                        File.Delete(objectFilePath);
                    }
                }

                // Delete the commits that are unique to the branch
                foreach (var commit in branchCommits)
                {
                    string commitFilePath = Path.Combine(paths.CommitDir, commit.Commit);
                    if (File.Exists(commitFilePath))
                    {
                        File.Delete(commitFilePath);
                    }
                }

                logger.Log($"Cleaned up files and commits from deleted branch.");
            }
            catch (Exception ex)
            {
                logger.Log($"Failed to delete branch {branchName}: {ex.Message}");

                RestoreBackupDeletion(paths, tempDeletionDir);

            }


            // Cleanup the temp deletion directory
            if (Directory.Exists(tempDeletionDir))
            {
                Directory.Delete(tempDeletionDir, true);
            }

        }

        private static void BackupDeletion(Paths paths, string tempDeletionDir, List<string>? filesToDelete, List<CommitMetadata>? branchCommits)
        {
            // Backup the files to be deleted into the temp directory
            foreach (var file in filesToDelete)
            {
                string objectFilePath = Path.Combine(paths.ObjectDir, file);

                if (File.Exists(objectFilePath))
                {
                    string destPath = Path.Combine(tempDeletionDir, "objects", file);

                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.Move(objectFilePath, destPath);
                }
            }

            // Backup the commits to be deleted into the temp directory
            foreach (var commit in branchCommits)
            {
                string commitFilePath = Path.Combine(paths.CommitDir, commit.Commit);
                if (File.Exists(commitFilePath))
                {
                    string destPath = Path.Combine(tempDeletionDir, "commits", commit.Commit);

                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.Move(commitFilePath, destPath);
                }
            }
        }

        private static void RestoreBackupDeletion(Paths paths, string tempDeletionDir)
        {
            // Restore files
            string backupObjectsDir = Path.Combine(tempDeletionDir, "objects");

            if (Directory.Exists(backupObjectsDir))
            {
                foreach (var filePath in Directory.GetFiles(backupObjectsDir, "*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(backupObjectsDir, filePath);
                    string originalPath = Path.Combine(paths.ObjectDir, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(originalPath));
                    File.Move(filePath, originalPath, true);
                }
            }

            // Restore commits
            string backupCommitsDir = Path.Combine(tempDeletionDir, "commits");
            if (Directory.Exists(backupCommitsDir))
            {
                foreach (var filePath in Directory.GetFiles(backupCommitsDir, "*", SearchOption.AllDirectories))
                {
                    string relativePath = Path.GetRelativePath(backupCommitsDir, filePath);
                    string originalPath = Path.Combine(paths.CommitDir, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(originalPath));
                    File.Move(filePath, originalPath, true);
                }
            }
        }



        public static List<CommitMetadata> GetAllCommits(ILogger logger, Paths paths)
        {
            var allCommits = new List<CommitMetadata>();

            try
            {
                // Get all the commits in the commit directory
                var commitFiles = Directory.GetFiles(paths.CommitDir);

                foreach (var commit in commitFiles)
                {
                    var commitData = JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(commit));

                    if (commitData != null)
                    {
                        allCommits.Add(commitData);
                    }
                }

            } catch (Exception ex)
            {
                logger.Log( $"Failed to get all commits: {ex.Message}");
            }

            return allCommits;
        }

        */


    }
}
