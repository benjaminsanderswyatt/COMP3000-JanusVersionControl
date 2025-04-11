using System.Text.RegularExpressions;

namespace Janus.Helpers.CommandHelpers
{

    public class DeleteBranchHelper
    {


        public static bool IsValidBranchName(string branchName)
        {
            if (string.IsNullOrWhiteSpace(branchName))
                return false;


            // ivalid characters: ~ ^ : ? / \ * [ ] \x00-\x1F \x7F ..
            var invalidCharsPattern = @"[~^:\?\\\*/\[\]\x00-\x1F\x7F]|(\.\.)";
            if (Regex.IsMatch(branchName, invalidCharsPattern))
                return false;

            return true;
        }



    }
}
