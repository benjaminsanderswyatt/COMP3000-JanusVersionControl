
using Janus.Helpers;
using Janus.Plugins;

namespace Janus
{
    public class HeadHelper
    {
        public static void SetHeadCommit(Paths paths, string commitHash, string branchName = null)
        {
            if (branchName == null)
            {
                branchName = CommandHelper.GetCurrentBranchName(paths);
            }

            string branchHeadPath = Path.Combine(paths.HeadsDir, branchName);

            File.WriteAllText(branchHeadPath, commitHash);
        }



    }
}
