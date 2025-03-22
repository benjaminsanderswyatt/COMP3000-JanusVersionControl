using Janus.API;
using Janus.Models;
using Janus.Plugins;
using System.Text.Json;

namespace Janus.Helpers
{
    public class RepoHelper
    {

        public static async Task<List<string>> CompareRemoteAndLocalRepos(Paths paths, string pat, string owner, string repoName, string branch)
        {
            var (success, latestRemoteCommitHash) = await ApiHelper.SendGetAsync($"{owner}/{repoName}/{branch}/latestcommit", pat);

            if (!success)
            {
                Console.WriteLine("Failed to get latest remote commit.");
                return new List<string>(); // Failed to get remote 
            }

            string latestLocalCommitHash = MiscHelper.GetCurrentHeadCommitHash(paths);

            if (string.IsNullOrEmpty(latestLocalCommitHash))
            {
                // Error occured: repo starts with 1 initial commit
                Console.WriteLine("Error: Could not determine the latest local commit.");
                return new List<string>();
            }


            if (string.IsNullOrEmpty(latestRemoteCommitHash))
            {
                // Remote repo branch doesnt exist or main hasnt yet been pushed to
                // Local is ahead by whole branch
                Console.WriteLine("Remote branch does not exist. The local branch is ahead by all its commits.");

                // ----------- TODO: Should keep in mind the parent branch and where its commits have been pushed ---
                return FindCommitsToPush(paths, latestLocalCommitHash, null);
            }


            // Compare hashes
            if (latestLocalCommitHash == latestRemoteCommitHash)
            {
                Console.WriteLine("Your local repository is up to date.");
                return new List<string>();
            }

            // Check if local is ahead (commits exist in local but not in remote)
            List<string> commitsToPush = FindCommitsToPush(paths, latestLocalCommitHash, latestRemoteCommitHash);
            if (commitsToPush.Count > 0)
            {
                Console.WriteLine("Local repository is ahead. Commits need to be pushed.");
                return commitsToPush;
            }

            return new List<string>();
        }
















        private static CommitMetadata LoadCommit(Paths paths, string commitHash)
        {
            string commitPath = Path.Combine(paths.CommitDir, commitHash);
            if (!File.Exists(commitPath))
            {
                return null;
            }

            string json = File.ReadAllText(commitPath);
            return JsonSerializer.Deserialize<CommitMetadata>(json);
        }



        public static List<string> FindCommitsToPush(Paths paths, string localHead, string remoteHead)
        {
            // TODO: handle the multiple parent commits scenario
            /*
            List<string> commitsToPush = new List<string>();

            string currentCommit = localHead;
            while (!string.IsNullOrEmpty(currentCommit))
            {
                if (currentCommit == remoteHead)
                {
                    // Found the remote commit in the local history
                    return commitsToPush;
                }

                commitsToPush.Add(currentCommit);
                var commitData = LoadCommit(paths, currentCommit);
                currentCommit = commitData?.Parents.FirstOrDefault();
            }

            Console.WriteLine("Local and remote branches have diverged.");

            */
            return new List<string>(); // Indicates a divergence
        }



    }
}
