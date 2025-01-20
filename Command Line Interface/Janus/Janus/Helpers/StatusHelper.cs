using Janus.Models;
using Janus.Plugins;
using System.Security.Cryptography;
using System.Text;

namespace Janus.Helpers
{
    public class StatusHelper
    {

        public static (List<string> stagedForCommitModified, List<string> stagedForCommitAdded, List<string> stagedForCommitDeleted) GetNotStagedUntracked(Dictionary<string, object> tree , Dictionary<string, string> stagedFiles)
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


        public static (List<string> notStaged, List<string> untracked) GetNotStagedUntracked(List<string> workingFiles, Dictionary<string, string> stagedFiles)
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

                string fileHash = AddHelper.ComputeHash_GivenFilepath(filePath);
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
