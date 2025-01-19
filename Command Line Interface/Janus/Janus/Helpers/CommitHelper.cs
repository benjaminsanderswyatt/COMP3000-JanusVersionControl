using Janus.Models;
using Janus.Plugins;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Janus.Helpers
{

    public class CommitHelper
    {
       
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
