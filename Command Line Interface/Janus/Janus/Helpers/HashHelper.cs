using System.Security.Cryptography;
using System.Text;

namespace Janus.Helpers
{
    public class HashHelper
    {
        public static string ComputeHash(byte[] contentBytes)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(contentBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string ComputeTreeHash(string content)
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
            byte[] combined = Encoding.UTF8.GetBytes(treeHash + commitMessage);

            return ComputeHash(combined);
        }


        public static string ComputeHashGivenRelFilepath(string workingDir, string relativeFilePath)
        {
            string fullpath = Path.Combine(workingDir, relativeFilePath);

            // Read file content
            byte[] content = File.ReadAllBytes(fullpath);

            // Compute the hash from the content
            string fileHash = ComputeHash(content);

            return fileHash;
        }

        public static string ComputeHashGivenFullFilepath(string fullFilePath)
        {
            // Read file content
            byte[] content = File.ReadAllBytes(fullFilePath);

            // Compute the hash from the content
            string fileHash = ComputeHash(content);

            return fileHash;
        }


        public static (string fileHash, byte[] content) ComputeHashAndGetContent(string workingDir, string relativeFilePath)
        {
            string fullpath = Path.Combine(workingDir, relativeFilePath);

            // Read file content
            byte[] content = File.ReadAllBytes(fullpath);

            // Compute the hash from the content
            string fileHash = ComputeHash(content);

            return (fileHash, content);
        }

    }
}
