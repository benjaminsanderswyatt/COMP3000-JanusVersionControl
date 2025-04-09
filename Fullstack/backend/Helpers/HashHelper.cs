using backend.DataTransferObjects;
using backend.DataTransferObjects.CLI;
using backend.Models;
using backend.Utils;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static backend.Utils.TreeBuilder;

namespace backend.Helpers
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

        public static string ComputeCommitHash(string parentHash, string branchName, string authorName, string authorEmail, DateTime date, string commitMessage, string treeHash)
        {
            // combine all inputs into one
            string combinedInput = $"{parentHash}{branchName}{authorName}{authorEmail}{date}{commitMessage}{treeHash}";

            return ComputeHash(combinedInput);
        }

    }
}
