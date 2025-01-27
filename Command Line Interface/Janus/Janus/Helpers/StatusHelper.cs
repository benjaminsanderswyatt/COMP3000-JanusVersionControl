using Janus.Plugins;

namespace Janus.Helpers
{
    public class StatusHelper
    {
        /*
        public static bool AreThereUncommittedChanges(Paths paths)
        {
            // Get the head commit hash
            string commitHash = CommandHelper.GetCurrentHEAD(paths);

            // Get the tree from commit hash
            Dictionary<string, object> tree = TreeHelper.GetTreeFromCommitHash(paths, commitHash);

            var stagedFiles = IndexHelper.LoadIndex(paths.Index);

            var (stagedForCommitModified, stagedForCommitAdded, stagedForCommitDeleted) = GetStaged(tree, stagedFiles);

            if (stagedForCommitModified.Any() || stagedForCommitAdded.Any() || stagedForCommitDeleted.Any())
            {
                return true;
            }


            var workingFiles = GetFilesHelper.GetAllFilesInDir(paths, paths.WorkingDir);

            var (notStaged, untracked) = GetNotStagedUntracked(paths.WorkingDir, workingFiles, stagedFiles);

            if (notStaged.Any() || untracked.Any())
            {
                return true;
            }

            return false;
        }
        */

        /*
        public static bool HasAnythingBeenStagedForCommit(Paths paths, string commitHash, Dictionary<string, string> stagedFiles)
        {
            // Get the tree from commit hash
            Dictionary<string, object> tree = TreeHelper.GetTreeFromCommitHash(paths, commitHash);

            var (stagedForCommitModified, stagedForCommitAdded, stagedForCommitDeleted) = GetStaged(tree, stagedFiles);

            if (stagedForCommitModified.Any() || stagedForCommitAdded.Any() || stagedForCommitDeleted.Any())
            {
                return true;
            }

            // Nothing to commit
            return false;
        }
        */
        /*
        public static (List<string> stagedForCommitModified, List<string> stagedForCommitAdded, List<string> stagedForCommitDeleted) GetStaged(Dictionary<string, object> tree, Dictionary<string, string> stagedFiles)
        {
            // Compare index to tree, if they differ they are changes to be committed (if index hash == "Deleted" then file is deleted otherwise modified)
            var stagedForCommitModified = new List<string>();
            var stagedForCommitAdded = new List<string>();
            var stagedForCommitDeleted = new List<string>();

            foreach (var indexEntry in stagedFiles)
            {
                string filePath = indexEntry.Key;
                string fileHash = indexEntry.Value;

                // Check if the file exists in the tree
                string treeHash = TreeHelper.GetHashFromTree(tree, filePath);

                // File is staged for commit
                if (treeHash == null || treeHash == "Deleted") // File isnt in tree (was deleted then readded)
                {
                    stagedForCommitAdded.Add(filePath);
                }
                else if (treeHash != fileHash)
                {
                    if (fileHash == "Deleted")
                    {
                        stagedForCommitDeleted.Add(filePath); // (deleted)
                    }
                    else
                    {
                        stagedForCommitModified.Add(filePath); // (modified)
                    }
                }

            }

            return (stagedForCommitModified, stagedForCommitAdded, stagedForCommitDeleted);
        }
        */

        public static (List<string> notStaged, List<string> untracked) GetNotStagedUntracked(string workingDir, IEnumerable<string> workingFiles, Dictionary<string, string> stagedFiles)
        {
            // Compare workingDir to index, if the files differ from index they are not staged for commit, if not in index they are untracked
            var notStaged = new List<string>();
            var untracked = new List<string>();

            foreach (var filePath in workingFiles)
            {

                if (!stagedFiles.ContainsKey(filePath))
                {
                    untracked.Add(filePath); // (untracked)
                    continue;
                }

                string fileHash = HashHelper.ComputeHashGivenRelFilepath(workingDir, filePath);
                if (fileHash != stagedFiles[filePath])
                {
                    notStaged.Add(filePath); // (not staged)

                }

            }

            return (notStaged, untracked);
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
