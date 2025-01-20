using Janus.Models;
using Janus.Plugins;
using System.Security.Cryptography;
using System.Text;

namespace Janus.Helpers
{
    public class AddHelper
    {
        public static bool IsFileIgnored(string filePath, IEnumerable<string> ignoredPatterns)
        {
            return ignoredPatterns.Any(pattern =>
                filePath.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }


    }
}
