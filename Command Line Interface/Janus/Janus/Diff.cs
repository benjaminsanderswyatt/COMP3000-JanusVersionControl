using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Janus.Helpers;
using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using System.Text;
using System.Text.Json;

namespace Janus
{
    public class Diff
    {
        public enum TreeSourceType
        {
            Working,
            Staged,
            Commit
        }



        public static void GetAndLogDiff(ILogger Logger, string fileName, string before, string after)
        {
            Logger.Log($"diff --janus {fileName}");
            var diff = InlineDiffBuilder.Diff(before, after);

            foreach (var line in diff.Lines)
            {
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Logger.Log($"+ {line.Text}");
                        break;
                    case ChangeType.Deleted:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Logger.Log($"- {line.Text}");
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Logger.Log($"  {line.Text}");
                        break;
                }
            }

            Console.ResetColor();
        }

        public static void DisplayDiffs(ILogger Logger, Paths paths, TreeComparisonResult result, TreeNode oldTree, TreeNode newTree, TreeSourceType oldSource, TreeSourceType newSource)
        {
            
            foreach (var file in result.AddedOrUntracked)
            {
                string newMime = GetMimeTypeFromTree(newTree, file);
                if (IsBinaryMimeType(newMime))
                {
                    Logger.Log($"Binary diff not supported: {file}");
                    MiscHelper.DisplaySeperator(Logger);
                    continue;
                }

                string oldContent = GetContentFromSource(paths, oldTree, oldSource, file);
                string newContent = GetContentFromSource(paths, newTree, newSource, file);

                GetAndLogDiff(Logger, file, oldContent, newContent);

                MiscHelper.DisplaySeperator(Logger);
            }

            foreach (var file in result.ModifiedOrNotStaged)
            {
                string oldMime = GetMimeTypeFromTree(oldTree, file);
                string newMime = GetMimeTypeFromTree(newTree, file);
                if (IsBinaryMimeType(oldMime) || IsBinaryMimeType(newMime))
                {
                    Logger.Log($"Binary diff not supported: {file}");
                    MiscHelper.DisplaySeperator(Logger);
                    continue;
                }

                string oldContent = GetContentFromSource(paths, oldTree, oldSource, file);
                string newContent = GetContentFromSource(paths, newTree, newSource, file);

                GetAndLogDiff(Logger, file, oldContent, newContent);

                MiscHelper.DisplaySeperator(Logger);
            }

            foreach (var file in result.Deleted)
            {
                string oldMime = GetMimeTypeFromTree(oldTree, file);
                if (IsBinaryMimeType(oldMime))
                {
                    Logger.Log($"Binary diff not supported: {file}");
                    MiscHelper.DisplaySeperator(Logger);
                    continue;
                }

                string oldContent = GetContentFromSource(paths, oldTree, oldSource, file);

                GetAndLogDiff(Logger, file, oldContent, string.Empty);

                MiscHelper.DisplaySeperator(Logger);
            }

        }





        public static string GetContentFromSource(Paths paths, TreeNode tree, TreeSourceType sourceType, string filePath)
        {
            try
            {
                switch (sourceType)
                {
                    case TreeSourceType.Working:
                        string fullPath = Path.Combine(paths.WorkingDir, filePath);
                        return File.Exists(fullPath) ? File.ReadAllText(fullPath) : string.Empty;

                    case TreeSourceType.Staged:
                    case TreeSourceType.Commit:
                        string hash = Tree.GetHashFromTree(tree, filePath);

                        if (string.IsNullOrEmpty(hash))
                            return string.Empty;

                        string objectPath = Path.Combine(paths.ObjectDir, hash);
                        return File.Exists(objectPath) ? File.ReadAllText(objectPath) : string.Empty;

                    default:
                        return string.Empty;
                }
            }
            catch (Exception)
            {
                return "<Binary Content>";
            }
            
        }

        public static TreeNode GetTreeFromCommit(ILogger logger, Paths paths, string commitHash)
        {
            string commitPath = Path.Combine(paths.CommitDir, commitHash);
            if (!File.Exists(commitPath))
            {
                logger.Log($"Commit '{commitHash}' not found.");
                return null;
            }

            var commitMetadata = JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(commitPath));
            var treeBuilder = new TreeBuilder(paths);

            return treeBuilder.RecreateTree(logger, commitMetadata.Tree);
        }



        public static string GetParentCommit(ILogger logger, Paths paths, string commitHash)
        {
            string commitPath = Path.Combine(paths.CommitDir, commitHash);
            if (!File.Exists(commitPath))
            {
                logger.Log($"Commit '{commitHash}' not found.");
                return null;
            }

            var commitMetadata = JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(commitPath));
            
            return commitMetadata.Parents.FirstOrDefault();
        }






        public static string GetMimeTypeFromTree(TreeNode tree, string relativePath)
        {
            string[] pathParts = PathHelper.PathSplitter(relativePath);
            TreeNode currentNode = tree;

            foreach (var part in pathParts)
            {
                currentNode = currentNode.Children.FirstOrDefault(c => c.Name == part);
                if (currentNode == null) 
                    break;
            }

            return currentNode?.MimeType;
        }

        private static bool IsBinaryMimeType(string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType)) return false;

            // Common non-text MIME types
            return !(
                mimeType.StartsWith("text/") ||
                mimeType == "application/json" ||
                mimeType == "application/xml" ||
                mimeType == "application/javascript"
            );
        }


    }
}