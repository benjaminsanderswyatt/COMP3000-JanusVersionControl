using Janus.Models;
using Janus.Plugins;
using Newtonsoft.Json;
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




        public static Dictionary<string, string> LoadIndex(string indexPath)
        {
            var index = new Dictionary<string, string>();
            if (File.Exists(indexPath))
            {
                foreach (var line in File.ReadAllLines(indexPath))
                {
                    var parts = line.Split('|');
                    if (parts.Length == 2)
                        index[parts[0].Trim()] = parts[1].Trim();
                }
            }
            return index;
        }

        public static void SaveIndex(string indexPath, Dictionary<string, string> stagedFiles)
        {
            File.WriteAllLines(indexPath, stagedFiles.Select(kv => $"{kv.Key}|{kv.Value}"));
        }


    }
}
