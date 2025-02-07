using Janus.Models;
using Janus.Plugins;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Janus.Helpers
{
    public class HashHelper
    {
        public static string ComputeHashBytes(byte[] contentBytes)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(contentBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string ComputeHash(string content)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                byte[] hashBytes = sha1.ComputeHash(contentBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string ComputeCommitHash(string parentHash, string branchName, string authorName, string authorEmail, DateTimeOffset date, string commitMessage, string treeHash)
        {
            // combine all inputs into one
            string combinedInput = $"{parentHash}{branchName}{authorName}{authorEmail}{date.ToUnixTimeSeconds()}{commitMessage}{treeHash}";

            return ComputeHash(combinedInput);
        }


        public static string ComputeHashGivenRelFilepath(string workingDir, string relativeFilePath)
        {
            string fullpath = Path.Combine(workingDir, relativeFilePath);

            // Read file content
            byte[] content = File.ReadAllBytes(fullpath);

            // Compute the hash from the content
            string fileHash = ComputeHashBytes(content);

            return fileHash;
        }

        public static string ComputeHashGivenFullFilepath(string fullFilePath)
        {
            // Read file content
            byte[] content = File.ReadAllBytes(fullFilePath);

            // Compute the hash from the content
            string fileHash = ComputeHashBytes(content);

            return fileHash;
        }


        public static (string fileHash, byte[] content) ComputeHashAndGetContent(string workingDir, string relativeFilePath)
        {
            string fullpath = Path.Combine(workingDir, relativeFilePath);

            // Read file content
            byte[] content = File.ReadAllBytes(fullpath);

            // Compute the hash from the content
            string fileHash = ComputeHashBytes(content);

            return (fileHash, content);
        }



        public static string GetTreeHashFromCommitHash(Paths paths, string commitHash)
        {
            string commitPath = Path.Combine(paths.CommitDir, commitHash);

            if (!File.Exists(commitPath))
            {
                throw new Exception("Error: Couldn't find commit");
            }

            string content = File.ReadAllText(commitPath);

            CommitMetadata commit = JsonSerializer.Deserialize<CommitMetadata>(content);

            return commit.Tree;
        }

    }
}
