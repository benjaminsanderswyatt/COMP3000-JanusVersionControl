
using Janus.Plugins;

namespace Janus
{
    public class HeadHelper
    {
        public static void SetHeadCommit(Paths paths, string commitHash)
        {
            var refHead = File.ReadAllText(paths.HEAD).Substring(5); // Remove the "ref: " at the start
            File.WriteAllText(Path.Combine(paths.JanusDir, refHead), commitHash);
        }



    }
}
