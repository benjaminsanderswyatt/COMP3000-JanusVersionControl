using Janus.Models;
using Janus.Plugins;
using System.Security.Cryptography;
using System.Text;

namespace Janus.Helpers
{
    public class AddHelper
    {
        public static bool IsFileIgnored(string filePath, IEnumerable<string> ignoredPatterns)
        {
            return ignoredPatterns.Any(pattern =>
                filePath.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }



        public static string ComputeHash_GivenFilepath(string filePath)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = sha1.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
            
        }



    }
}
