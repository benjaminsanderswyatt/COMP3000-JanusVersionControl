using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Janus
{
    internal class CommandHelper
    {
        
        public static string ComputeHash(string content)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                byte[] hashBytes = sha1.ComputeHash(contentBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "");
            }
        }

        public static string ComputeCommitHash(Dictionary<string, string> fileHashes, string commitMessage)
        {
            string combined = string.Join("", fileHashes.Select(kv => kv.Key + kv.Value)) + commitMessage;
            return ComputeHash(combined);
        }

        public static string GetCurrentHead()
        {
            string headPath = Paths.head;

            if (File.Exists(headPath))
            {
                return File.ReadAllText(headPath).Trim();
            }

            return string.Empty; // Empty means this is the initial commit
        }


        public static string GenerateCommitMetadata(string commitHash, Dictionary<string, string> fileHashes, string commitMessage, string parentCommit)
        {
            var metadata = new StringBuilder();

            metadata.AppendLine($"Commit: {commitHash}");
            metadata.AppendLine($"Parent: {parentCommit}");
            metadata.AppendLine($"Date: {DateTime.UtcNow}");
            metadata.AppendLine($"Message: {commitMessage}");
            metadata.AppendLine("Files:");

            foreach (var kv in fileHashes)
            {
                metadata.AppendLine($"  {kv.Key} -> {kv.Value}");
            }

            return metadata.ToString();
        }

    }
}
