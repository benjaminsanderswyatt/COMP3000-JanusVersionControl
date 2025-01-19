using Janus.Models;
using Janus.Plugins;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Janus.Helpers
{
    public class TreeHelper
    {

        public static void AddToTreeRecursive(Dictionary<string, object> tree, string[] pathParts, string fileHash)
        {
            if (pathParts.Length == 1)
            {
                // Base case: add the file to the current tree level
                tree[pathParts[0]] = fileHash;
            }
            else
            {
                // Recursive case: traverse or create subdirectories
                string folderName = pathParts[0];
                if (!tree.ContainsKey(folderName))
                {
                    tree[folderName] = new Dictionary<string, object>();
                }

                var subTree = tree[folderName] as Dictionary<string, object>;
                AddToTreeRecursive(subTree, pathParts.Skip(1).ToArray(), fileHash);
            }
        }

        public static Dictionary<string, object> GetTreeFromCommitHash(Paths paths, string commitHash)
        {
            // Get contents of commit with the hash
            string contents = File.ReadAllText(Path.Combine(paths.CommitDir, commitHash));

            // Deserialise into commitMetadata
            CommitMetadata metadata = JsonSerializer.Deserialize<CommitMetadata>(contents);
            
            string tree = File.ReadAllText(Path.Combine(paths.TreeDir, metadata.Tree));

            var treeObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(tree);

            var parsedTree = ParseTree(treeObject);

            return parsedTree;
        }


        private static Dictionary<string, object> ParseTree(Dictionary<string, JsonElement> tree)
        {
            var result = new Dictionary<string, object>();

            foreach (var kvp in tree)
            {
                if (kvp.Value.ValueKind == JsonValueKind.Object)
                {
                    // If it's an object, recursively parse it
                    var nestedObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(kvp.Value.GetRawText());
                    result[kvp.Key] = ParseTree(nestedObject);
                }
                else if (kvp.Value.ValueKind == JsonValueKind.String)
                {
                    // If it's a string, it represents a file hash
                    result[kvp.Key] = kvp.Value.GetString();
                }
            }

            return result;
        }



        public static string GetHashFromTree(Dictionary<string, object> tree, string filePath)
        {
            string[] pathParts = filePath.Split(Path.DirectorySeparatorChar);

            Dictionary<string, object> currentTree = tree;


            for (int i = 0; i <= pathParts.Length; i++)
            {
                string part = pathParts[i];

                if (!currentTree.ContainsKey(part))
                {
                    Console.WriteLine($"Key '{part}' not found in currentTree. Available keys: {string.Join(", ", currentTree.Keys)}");
                    // The file or folder does not exist in the tree
                    return null;
                }
                

                if (i == pathParts.Length - 1)
                {
                    // If we are at the last part of the path, it should be a file
                    return currentTree[part] as string; // Found the hash
                    
                }
                else
                {
                    // Traverse to the next level in the tree (folder)
                    if (currentTree[part] is Dictionary<string, object> nextTree)
                    {
                        currentTree = nextTree;
                    }
                    else
                    {
                        // The path doesn't match a folder structure
                        return null;
                    }
                }
                
            }

            // An error occured if your here
            return null;
        }





    }
}
