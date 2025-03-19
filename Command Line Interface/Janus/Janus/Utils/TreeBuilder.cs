using Janus.DataTransferObjects;
using Janus.Helpers;
using Janus.Helpers.CommandHelpers;
using Janus.Plugins;
using System.Drawing;
using System.Text;
using static Janus.Helpers.FileMetadataHelper;

namespace Janus.Utils
{
    public class TreeNode
    {
        public string Name { get; set; } // Name of the file or directory
        public string? Hash { get; set; } // Hash of the file (null for directory)
        public string? MimeType { get; set; } // Mime type of the file
        public long Size { get; set; } // Size of the file
        public DateTimeOffset LastModified { get; set; } // Last modified date
        public List<TreeNode> Children { get; set; } // List of child nodes

        public TreeNode(string name, string hash = null, string mimeType = "inode/directory", long size = 0, DateTimeOffset? lastModified = null)
        {
            Name = name;
            Hash = hash;
            MimeType = mimeType;
            Size = size;
            LastModified = lastModified ?? DateTimeOffset.MinValue;
            Children = new List<TreeNode>();
        }

    }

    public class TreeBuilder
    {
        private readonly Paths _paths;
        public TreeNode root;

        public TreeBuilder(Paths paths)
        {
            _paths = paths;
            root = new TreeNode("root"); // Root of the tree
        }


        // Build the tree from a dictionary of filepath hash
        public TreeNode BuildTreeFromDiction(Dictionary<string, FileMetadata> index)
        {

            foreach (var kvp in index)
            {
                string filePath = kvp.Key;
                FileMetadata meta = kvp.Value;


                string[] pathParts = PathHelper.PathSplitter(filePath);
                long addedSize = AddToTree(
                    pathParts,
                    meta.Hash,
                    meta.MimeType,
                    meta.Size,
                    meta.LastModified,
                    0,
                    root
                );

                root.Size += addedSize; // Handle root size
            }

            ComputeDirectoryLastModified(root); // Compute the last modified date for dirs

            return root;
        }

        private void ComputeDirectoryLastModified(TreeNode node)
        {
            if (node == null || node.Hash != null) return;

            DateTimeOffset maxLastModified = DateTimeOffset.MinValue;
            foreach (var child in node.Children)
            {
                ComputeDirectoryLastModified(child);
                if (child.LastModified > maxLastModified)
                {
                    maxLastModified = child.LastModified;
                }
            }
            node.LastModified = maxLastModified;
        }


        public static DateTimeOffset GetLastModifiedFromTree(TreeNode tree, string relativePath)
        {
            string[] pathParts = PathHelper.PathSplitter(relativePath);
            TreeNode currentNode = tree;

            foreach (var part in pathParts)
            {
                currentNode = currentNode.Children.FirstOrDefault(c => c.Name == part);
                if (currentNode == null) return DateTimeOffset.MinValue;
            }

            return currentNode.LastModified;
        }


        private long AddToTree(string[] pathParts, string hash, string mimeType, long size, DateTimeOffset lastModified, int index, TreeNode current)
        {
            string part = pathParts[index]; // Get the current part of the path
            bool isFile = index == pathParts.Length - 1; // Last part of the path is the file


            // Find and or create the child node with the current part
            var child = current.Children.Find(c => c.Name == part);
            if (child == null)
            {

                child = new TreeNode(
                    part,
                    isFile ? hash : null,
                    isFile ? mimeType : "inode/directory",
                    0, // Handle size later
                    isFile ? lastModified : DateTimeOffset.MinValue 
                );
                current.Children.Add(child);
            }

            long addedSize;
            if (isFile)
            {
                child.LastModified = lastModified;
                child.Size = size;
                addedSize = size;
            }
            else
            {
                // Recursively add the file
                addedSize = AddToTree(pathParts, hash, mimeType, size, lastModified, index + 1, child);
                child.Size += addedSize;
            }


            return addedSize;
        }


        public void PrintTree(TreeNode node = null, int level = 0)
        {
            node ??= root; // Start from the root if no node is provided

            string indent = new string(' ', level * 2);

            string type = node.Hash != null ? "File" : "Dir";

            Console.WriteLine($"{indent}{node.Name} | {node.MimeType} | {node.Size} | {node.Hash}");

            foreach (var child in node.Children)
            {
                PrintTree(child, level + 1);
            }
        }


        public string SaveTree()
        {
            return SaveTreeRecursively(root);
        }

        private string SaveTreeRecursively(TreeNode node)
        {
            var entries = node.Children
            .OrderBy(c => c.Name)
            .Select(child => child.Hash == null
                ? $"tree|{child.Name}|{child.MimeType}|{child.Size}|{SaveTreeRecursively(child)}|{child.LastModified.ToString("o")}"
                : $"blob|{child.Name}|{child.MimeType}|{child.Size}|{child.Hash}|{child.LastModified.ToString("o")}");

            // Create the content for this directory
            string content = string.Join("\n", entries);
            string hash = HashHelper.ComputeHash(content);

            // Save the directory file (dont overide if already exists)
            string filePath = Path.Combine(_paths.TreeDir, hash);

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, content);
            }

            return hash;
        }

        public TreeNode RecreateTree(ILogger logger, string treeHash)
        {
            if (string.IsNullOrEmpty(treeHash)) // Initial commit
            {
                return new TreeNode("root");
            }

            TreeNode node = RebuildTreeRecursive(logger, treeHash);

            root = node;
            return node;
        }


        private TreeNode RebuildTreeRecursive(ILogger logger, string treeHash)
        {
            try
            {
                // Load the tree content from storage
                string treePath = Path.Combine(_paths.TreeDir, treeHash);
                string[] treeContent = File.ReadAllLines(treePath, Encoding.UTF8);


                // Create a new TreeNode for the current tree
                var node = new TreeNode("root", mimeType: "inode/directory");

                foreach (var line in treeContent)
                {
                    var parts = line.Split('|');
                    if (parts.Length != 6) 
                        continue;


                    string type = parts[0]; // blob or tree
                    string name = parts[1]; // Name of the file or directory
                    string mimeType = parts[2]; // Mime of the file (directory = null)
                    long size = long.Parse(parts[3]); // Size of the file or directory
                    string hash = parts[4]; // Hash of the file or directory
                    DateTimeOffset lastModified = DateTimeOffset.Parse(parts[5]); // Last modified date

                    var child = type == "tree"
                        ? RebuildTreeRecursive(logger, hash)
                        : new TreeNode(name, hash, mimeType, size, lastModified);

                    
                   
                    child.Name = name;
                    child.MimeType = mimeType;
                    child.Size = size;
                    child.LastModified = lastModified;

                    node.Children.Add(child);
                    node.Size += size;

                }


                return node;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to recreate tree", ex);
                //return null;
            }
        }

        public void LoadTree(TreeDto treeDto)
        {
            root = ConvertTreeDtoToTreeNode(treeDto);
        }

        public static TreeNode ConvertTreeDtoToTreeNode(TreeDto treeDto)
        {
            if (treeDto == null)
                return null;

            var node = new TreeNode(
                treeDto.Name,
                treeDto.Hash,
                treeDto.MimeType,
                treeDto.Size,
                treeDto.LastModified
            );

            if (treeDto.Children != null)
            {
                foreach (var childDto in treeDto.Children)
                {
                    node.Children.Add(ConvertTreeDtoToTreeNode(childDto));
                }
            }

            return node;
        }


        // Gets the file hashes from the tree
        public void GetFileHashes(HashSet<string> hashes)
        {
            GetFileHashesRecursive(root, hashes);
        }

        private static void GetFileHashesRecursive(TreeNode node, HashSet<string> hashes)
        {
            if (node == null)
                return;

            if (!string.IsNullOrEmpty(node.Hash))
            {
                // Is file
                hashes.Add(node.Hash);
            }

            foreach (var child in node.Children)
            {
                GetFileHashesRecursive(child, hashes);
            }
        }




        public Dictionary<string, FileMetadata> BuildIndexDictionary()
        {
            var index = new Dictionary<string, FileMetadata>();
            BuildIndexDictionaryRecursive(root, "", index);
            return index;
        }

        private void BuildIndexDictionaryRecursive(TreeNode node, string currentPath, Dictionary<string, FileMetadata> index)
        {
            foreach (var child in node.Children)
            {
                string path = Path.Combine(currentPath, child.Name);

                if (child.Hash != null)
                {
                    index[path] = new FileMetadata
                    {
                        Hash = child.Hash,
                        MimeType = child.MimeType,
                        Size = child.Size
                    };
                }
                else
                {
                    BuildIndexDictionaryRecursive(child, path, index);
                }
            }
        }










    }


    // Represents the results of comparing two trees
    public class TreeComparisonResult
    {
        public List<string> AddedOrUntracked { get; set; } = new(); // GetAddedModifiedDeleted -> Added, GetNotStagedUntracked -> Untracked
        public List<string> ModifiedOrNotStaged { get; set; } = new(); // GetAddedModifiedDeleted -> Modified, GetNotStagedUntracked -> Not Staged
        public List<string> Deleted { get; set; } = new();
    }

    public static class Tree
    {
        public static TreeComparisonResult CompareTrees(TreeNode tree1, TreeNode tree2)
        {
            var result = new TreeComparisonResult();

            if (tree1 == null && tree2 == null)
            {
                // Both trees are empty -> no changes
                return result;
            }

            if (tree1 == null)
            {
                // All files in tree2 are added
                CollectPaths(tree2, "", result.AddedOrUntracked);
                return result;
            }

            if (tree2 == null)
            {
                // All files in tree1 are deleted
                CollectPaths(tree1, "", result.Deleted);
                return result;
            }

            // Compare
            CompareNodes(tree1, tree2, "", result);

            return result;
        }


        private static void CollectPaths(TreeNode node, string currentPath, List<string> result)
        {
            if (node == null) return;

            foreach (var child in node.Children)
            {
                string path = Path.Combine(currentPath, child.Name);

                if (child.Hash != null)
                {
                    // Add file path to the result
                    result.Add(path);
                }
                else
                {
                    // Recurse into sub directories
                    CollectPaths(child, path, result);
                }
            }
        }


        private static void CompareNodes(TreeNode node1, TreeNode node2, string currentPath, TreeComparisonResult result)
        {
            // Collect children as dictionaries for easier lookup
            var node1Files = node1?.Children?.Where(c => c.Hash != null).ToDictionary(c => c.Name, c => c) ?? new Dictionary<string, TreeNode>();
            var node2Files = node2?.Children?.Where(c => c.Hash != null).ToDictionary(c => c.Name, c => c) ?? new Dictionary<string, TreeNode>();

            // Combine keys of all files in both trees
            var allKeys = new HashSet<string>(node1Files.Keys.Concat(node2Files.Keys));

            foreach (var key in allKeys)
            {
                string path = Path.Combine(currentPath, key);

                if (!node1Files.ContainsKey(key)) // Added
                {
                    // File is only in tree2
                    result.AddedOrUntracked.Add(path);
                }
                else if (!node2Files.ContainsKey(key) || node2Files[key].Hash == "Deleted") // Deleted
                {
                    // File is only in tree1 (not in tree2)
                    result.Deleted.Add(path);
                }
                else
                {
                    // File exists in both
                    var file1 = node1Files[key];
                    var file2 = node2Files[key];

                    //Modified
                    if (file1.Hash != file2.Hash ||
                        file1.MimeType != file2.MimeType ||
                        file1.Size != file2.Size)
                    {
                        result.ModifiedOrNotStaged.Add(path);
                    }
                }
            }

            // Recurse into subdirectories ignore empty folders
            var node1Dirs = node1?.Children?.Where(c => c.Hash == null) ?? Enumerable.Empty<TreeNode>();
            var node2Dirs = node2?.Children?.Where(c => c.Hash == null) ?? Enumerable.Empty<TreeNode>();

            var allDirs = new HashSet<string>(node1Dirs.Select(d => d.Name).Concat(node2Dirs.Select(d => d.Name)));

            foreach (var dirName in allDirs)
            {
                var childNode1 = node1Dirs.FirstOrDefault(d => d.Name == dirName);
                var childNode2 = node2Dirs.FirstOrDefault(d => d.Name == dirName);

                CompareNodes(childNode1, childNode2, Path.Combine(currentPath, dirName), result);
            }
        }




        public static TreeNode GetStagedTree(Paths paths)
        {
            var stagedFiles = IndexHelper.LoadIndex(paths.Index);
            var stagedTreeBuilder = new TreeBuilder(paths);
            var stagedTree = stagedTreeBuilder.BuildTreeFromDiction(stagedFiles);

            return stagedTree;
        }


        public static TreeNode GetHeadTree(ILogger logger, Paths paths, string branchName = null)
        {
            string commitHash;

            // Get the head tree of the current branch if not provided
            if (branchName == null)
            {
                commitHash = MiscHelper.GetCurrentHeadCommitHash(paths);
            }
            else
            {
                commitHash = BranchHelper.GetBranchHead(paths, branchName);
            }


            string treeHash = HashHelper.GetTreeHashFromCommitHash(paths, commitHash);

            var treeBuilder = new TreeBuilder(paths);
            var headTree = treeBuilder.RecreateTree(logger, treeHash);

            return headTree;
        }

        public static TreeNode GetWorkingTree(Paths paths, Dictionary<string, FileMetadata> workingDirFiles = null)
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


        public static string GetHashFromTree(TreeNode tree, string relativePath)
        {
            string[] pathParts = PathHelper.PathSplitter(relativePath);
            TreeNode currentNode = tree;

            foreach (var part in pathParts)
            {
                currentNode = currentNode.Children.FirstOrDefault(c => c.Name == part);
                if (currentNode == null) 
                    break;
            }

            return currentNode?.Hash;
        }




    }


}
