using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using System.Text.Json;

namespace Janus.Helpers
{
    public class StatusHelper
    {
        // stagedTree and headTree are optional
        public static bool HasAnythingBeenStagedForCommit(ILogger logger, Paths paths, TreeNode stagedTree = null, TreeNode headTree = null)
        {
            var comparisonResult = GetAddedModifiedDeleted(logger, paths, stagedTree, headTree);

            if (comparisonResult.AddedOrUntracked.Any() || comparisonResult.ModifiedOrNotStaged.Any() || comparisonResult.Deleted.Any())
            {
                return true;
            }

            return false;
        }


        public static bool AreThereUncommittedChanges(ILogger logger, Paths paths, TreeNode stagedTree = null, TreeNode workingTree = null)
        {
            var addedModifiedDeleted = GetAddedModifiedDeleted(logger, paths, stagedTree);

            if (addedModifiedDeleted.AddedOrUntracked.Any() || addedModifiedDeleted.ModifiedOrNotStaged.Any() || addedModifiedDeleted.Deleted.Any())
            {
                return true;
            }


            var notStagedUntracked = GetNotStagedUntracked(paths, stagedTree, workingTree);

            if (notStagedUntracked.ModifiedOrNotStaged.Any() || notStagedUntracked.AddedOrUntracked.Any())
            {
                return true;
            }

            return false;
        }







        // Compare head to tree
        public static TreeComparisonResult GetAddedModifiedDeleted(ILogger logger, Paths paths, TreeNode stagedTree = null, TreeNode headTree = null)
        {
            // Load staged tree if not provided
            if (stagedTree == null)
            {
                stagedTree = GetStagedTree(paths);
            }


            // Load head tree if not provided
            if (headTree == null)
            {
                headTree = GetHeadTree(logger, paths);
            }


            // Compare head to staged tree (differances are changes to be committed)
            var comparisonResult = Tree.CompareTrees(headTree, stagedTree);

            return comparisonResult;
        }

        // Compare staged to working
        public static TreeComparisonResult GetNotStagedUntracked(Paths paths, TreeNode stagedTree = null, TreeNode workingTree = null)
        {
            // Load staged tree if not provided
            if (stagedTree == null)
            {
                stagedTree = GetStagedTree(paths);
            }

            // Load working tree if not provided
            if (workingTree == null)
            {
                workingTree = GetWorkingTree(paths);
            }


            // Compare staged tree to working tree (differances are not staged for commit or untracked)
            var comparisonResult = Tree.CompareTrees(stagedTree, workingTree);

            return comparisonResult;
        }



        public static TreeNode GetStagedTree(Paths paths)
        {
            var stagedFiles = IndexHelper.LoadIndex(paths.Index);
            var stagedTreeBuilder = new TreeBuilder(paths);
            var stagedTree = stagedTreeBuilder.BuildTreeFromDiction(stagedFiles);

            return stagedTree;
        }


        public static TreeNode GetHeadTree(ILogger logger, Paths paths)
        {
            string commitHash = CommandHelper.GetCurrentHEAD(paths);
            string commitFilePath = Path.Combine(paths.CommitDir, commitHash);
            CommitMetadata commitMetadata = JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(commitFilePath));

            var treeBuilder = new TreeBuilder(paths);
            var headTree = treeBuilder.RecreateTree(logger, commitMetadata.Tree);

            return headTree;
        }


        public static TreeNode GetWorkingTree(Paths paths, Dictionary<string, string> workingDirFiles = null)
        {
            // Load working directory files if not provided
            if (workingDirFiles == null)
            {
                workingDirFiles = GetFilesHelper.GetWorkingDirFileHash(paths);
            }


            var workingTreeBuilder = new TreeBuilder(paths);
            var workingTree = workingTreeBuilder.BuildTreeFromDiction(workingDirFiles);

            return workingTree;
        }




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
