using Janus.Utils;
using static Janus.Helpers.FileMetadataHelper;

namespace Janus.Helpers
{
    public class IndexHelper
    {

        public static Dictionary<string, FileMetadata> LoadIndex(string indexPath)
        {
            var index = new Dictionary<string, FileMetadata>();
            if (File.Exists(indexPath))
            {
                foreach (var line in File.ReadAllLines(indexPath))
                {
                    var parts = line.Split('|');
                    if (parts.Length == 4)
                    {
                        index[parts[0].Trim()] = new FileMetadata
                        {
                            Hash = parts[1].Trim(),
                            MimeType = parts[2].Trim(),
                            Size = long.Parse(parts[3].Trim())
                        };
                    }
                }
            }
            return index;
        }


        public static void SaveIndex(string indexPath, Dictionary<string, FileMetadata> stagedFiles)
        {
            var lines = stagedFiles.Select(kv =>
                $"{kv.Key}|{kv.Value.Hash}|{kv.Value.MimeType}|{kv.Value.Size}");

            File.WriteAllLines(indexPath, lines);
        }


    }
}
