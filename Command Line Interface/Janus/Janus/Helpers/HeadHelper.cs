
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
                branchName = MiscHelper.GetCurrentBranchName(paths);
            }

            string branchHeadPath = Path.Combine(paths.BranchesDir, branchName, "head");

            File.WriteAllText(branchHeadPath, commitHash);
        }



    }
}
