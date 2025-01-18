using Janus.Models;
using Janus.Plugins;
using System.Security.Cryptography;
using System.Text;

namespace Janus.Helpers
{
    public class IndexHelper
    {

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
