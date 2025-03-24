using Janus.Helpers;
using Janus.Helpers.CommandHelpers;
using Janus.Plugins;
using Janus.Utils;
using static Janus.Helpers.FileMetadataHelper;

namespace Janus
{
    public class MergeHelper
    {

        public static string FindCommonAncestor(ILogger logger, Paths paths, string commitA, string commitB)
        {
            var visited = new HashSet<string>();
            var queue = new Queue<string>();
            queue.Enqueue(commitA);
            queue.Enqueue(commitB);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (visited.Contains(current))
                    return current;

                visited.Add(current);

                var commit = RepoHelper.LoadCommit(paths, current);

                if (commit.Parents != null)
                {
                    foreach (var parent in commit.Parents)
                    {
                        if (!string.IsNullOrEmpty(parent))
                            queue.Enqueue(parent);
                    }
                }
            }
            return null;
        }



        public static MergeResult ComputeMergeChanges(ILogger logger, Paths paths, TreeNode baseTree, TreeNode currentTree, TreeNode targetTree)
        {
            var result = new MergeResult();

            var baseCurrentDiff = Tree.CompareTrees(baseTree, currentTree);
            var baseTargetDiff = Tree.CompareTrees(baseTree, targetTree);

            // Find modified files in both branches
            var allFiles = baseCurrentDiff.AddedOrUntracked
                .Concat(baseCurrentDiff.ModifiedOrNotStaged)
                .Concat(baseCurrentDiff.Deleted)
                .Concat(baseTargetDiff.AddedOrUntracked)
                .Concat(baseTargetDiff.ModifiedOrNotStaged)
                .Concat(baseTargetDiff.Deleted)
                .Distinct();


            foreach (var file in allFiles)
            {
                var baseVersion = Tree.GetHashFromTree(baseTree, file);
                var currentVersion = Tree.GetHashFromTree(currentTree, file);
                var targetVersion = Tree.GetHashFromTree(targetTree, file);

                if (currentVersion == targetVersion)
                {
                    // No confilt -> add to merged
                    var meta = Tree.GetMetadataFromTree(currentTree, file);
                    if (meta != null) result.MergedEntries[file] = meta;
                    continue;
                }

                if (baseVersion == currentVersion)
                {
                    // Take targets changes
                    var meta = Tree.GetMetadataFromTree(targetTree, file);
                    if (meta != null) result.MergedEntries[file] = meta;
                }
                else if (baseVersion == targetVersion)
                {
                    // Take current changes
                    var meta = Tree.GetMetadataFromTree(currentTree, file);
                    if (meta != null) result.MergedEntries[file] = meta;
                }
                else
                {
                    // Conflict
                    result.Conflicts.Add(file);
                    result.HasConflicts = true;
                }


            }

            return result;

        }

        public class MergeResult
        {
            public Dictionary<string, FileMetadata> MergedEntries { get; } = new();
            public List<string> Conflicts { get; } = new();
            public bool HasConflicts { get; set; }
        }









        public static void RecreateWorkingDir(ILogger logger, Paths paths, TreeNode tree)
        {
            foreach (var child in tree.Children)
            {
                RecreateNode(logger, paths, child, "");
            }
        }

        private static void RecreateNode(ILogger logger, Paths paths, TreeNode node, string relPath)
        {
            string targetPath = Path.Combine(paths.WorkingDir, relPath, node.Name);


            if (node.Hash == null) // Directory
            {
                try
                {
                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log($"Failed to create directory '{targetPath}'");
                    logger.Log($"Error: {ex.Message}");
                    return;
                }

                // Recursive for all children
                string newRelPath = Path.Combine(relPath, node.Name);

                foreach (var child in node.Children)
                {
                    RecreateNode(logger, paths, child, newRelPath);
                }

            }
            else // File
            {

                string objectFilePath = Path.Combine(paths.ObjectDir, node.Hash);
                
                if (!File.Exists(objectFilePath))
                {
                    logger.Log($"Object file '{node.Name}' not found");
                    return;
                }

                try
                {
                    string fileDir = Path.GetDirectoryName(targetPath);

                    if (!Directory.Exists(fileDir))
                    {
                        Directory.CreateDirectory(fileDir);
                    }

                    byte[] fileContents = File.ReadAllBytes(objectFilePath);
                    File.WriteAllBytes(targetPath, fileContents);
                }
                catch (Exception ex)
                {
                    logger.Log($"Failed to create file '{targetPath}'");
                    logger.Log($"Error: {ex.Message}");
                }
            }

        }



    }
}
