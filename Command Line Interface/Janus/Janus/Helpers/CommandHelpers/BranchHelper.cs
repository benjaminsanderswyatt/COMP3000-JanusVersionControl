using Janus.Plugins;
using System.Text.RegularExpressions;

namespace Janus.Helpers.CommandHelpers
{

    public class BranchHelper
    {
        public static bool IsValidRepoOrBranchName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (name.Length < 10)
                return false;

            // ivalid characters: apace, ~ ^ : ? / \ * [ ] \x00-\x1F \x7F ..
            var invalidCharsPattern = @"[ ~^:\?\\\*/\[\]\x00-\x1F\x7F]|(\.\.)";
            if (Regex.IsMatch(name, invalidCharsPattern))
                return false;

            return true;
        }

        // Get the head commit hash from the branch
        public static string GetBranchHead(Paths paths, string branchName)
        {
            string headPath = Path.Combine(paths.BranchesDir, branchName, "head");

            if (!File.Exists(headPath))
            {
                throw new Exception("Error: Couldn't find branch head");
            }

            string targetCommitHash = File.ReadAllText(headPath);

            return targetCommitHash;
        }


        public static void SetCurrentHEAD(Paths paths, string branchName)
        {
            File.WriteAllText(paths.HEAD, $"ref: {paths.BranchesDir}/{branchName}/head");
        }







        public static Dictionary<string, string> GetAllBranchesLatestHashes(string branchesDir)
        {
            var hashes = new Dictionary<string, string>();

            foreach (var branchDir in Directory.GetDirectories(branchesDir))
            {
                string branchName = Path.GetFileName(branchDir);
                string headFile = Path.Combine(branchDir, "head");
                if (File.Exists(headFile))
                {
                    hashes[branchName] = File.ReadAllText(headFile).Trim();
                }
            }

            return hashes;
        }




    }
}
