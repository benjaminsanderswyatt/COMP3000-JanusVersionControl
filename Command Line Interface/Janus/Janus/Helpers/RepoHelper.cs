using Janus.Models;
using Janus.Plugins;
using System.Text.Json;

namespace Janus.Helpers
{
    public class RepoHelper
    {

        public static CommitMetadata LoadCommit(Paths paths, string commitHash)
        {
            string commitPath = Path.Combine(paths.CommitDir, commitHash);
            if (!File.Exists(commitPath))
            {
                return null;
            }

            string json = File.ReadAllText(commitPath);
            return JsonSerializer.Deserialize<CommitMetadata>(json);
        }





















    }
}
