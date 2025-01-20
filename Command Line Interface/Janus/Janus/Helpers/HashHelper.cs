using Janus.Models;
using Janus.Plugins;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace Janus.Helpers
{
    public class HashHelper
    {
        public static string ComputeHash(string content)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                byte[] hashBytes = sha1.ComputeHash(contentBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string ComputeCommitHash(string treeHash, string commitMessage)
        {
            string combined = treeHash + commitMessage;

            return ComputeHash(combined);
        }


        public static string ComputeHashGivenFilepath(string workingDir, string relativeFilePath)
        {
            string fullpath = Path.Combine(workingDir, relativeFilePath);

            // Read file content
            string content = File.ReadAllText(fullpath);

            // Compute the hash from the content
            string fileHash = ComputeHash(content);

            return fileHash;
        }


        public static (string fileHash, string content) ComputeHashAndGetContent(string workingDir, string relativeFilePath)
        {
            string fullpath = Path.Combine(workingDir, relativeFilePath);

            // Read file content
            string content = File.ReadAllText(fullpath);

            // Compute the hash from the content
            string fileHash = ComputeHash(content);

            return (fileHash, content);
        }

    }
}
