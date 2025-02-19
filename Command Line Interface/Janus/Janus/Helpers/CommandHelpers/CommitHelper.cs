using Janus.Plugins;

namespace Janus.Helpers.CommandHelpers
{

    public class CommitHelper
    {
        public static void SaveCommit(Paths paths, string commitHash, string parentCommit, string branch, string username, string email, DateTime datetime, string message, string rootTreeHash)
        {
            string commitMetadata = MiscHelper.GenerateCommitMetadata(branch, commitHash, rootTreeHash, message, parentCommit, username, email);

            // Save commit object
            string commitFilePath = Path.Combine(paths.CommitDir, commitHash);
            File.WriteAllText(commitFilePath, commitMetadata);
        }



        public static bool ValidateCommitMessage(ILogger Logger, string[] args, out string commitMessage)
        {
            commitMessage = args.Length > 0 ? args[0] : string.Empty;

            if (string.IsNullOrWhiteSpace(commitMessage))
            {
                Logger.Log("No commit message provided. Use 'janus commit <message>'.");
                return false;
            }

            if (commitMessage.Length > 256)
            {
                Logger.Log("Commit message is too long. Maximum length is 256 characters.");
                return false;
            }

            return true;
        }


    }
}
