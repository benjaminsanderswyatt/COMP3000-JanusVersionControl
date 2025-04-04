﻿using Janus.Plugins;

namespace Janus.Helpers.CommandHelpers
{

    public class CommitHelper
    {
        public static void SaveCommit(Paths paths, string commitHash, List<string> parentCommit, string branch, string username, string email, DateTimeOffset datetime, string message, string rootTreeHash)
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
                Logger.Log("No commit message provided. Use 'janus commit <message>'");
                return false;
            }

            if (commitMessage.Length > 256)
            {
                Logger.Log("Commit message is too long. Maximum length is 256 characters");
                return false;
            }

            return true;
        }







        public static string CreateMergeCommit(Paths paths, string currentHead, string targetHead, string treeHash, string message, string username, string email)
        {
            // Ensure order of parents is consistent
            var parents = new List<string> { currentHead, targetHead };

            DateTime now = DateTime.UtcNow;

            string commitHash = HashHelper.ComputeCommitHash(
                string.Join("|", parents),
                MiscHelper.GetCurrentBranchName(paths),
                username,
                email,
                now,
                message,
                treeHash
                );


            SaveCommit(
                paths,
                commitHash,
                parents,
                MiscHelper.GetCurrentBranchName(paths),
                username,
                email,
                now,
                message,
                treeHash
                );

            return commitHash;
        }



    }
}
