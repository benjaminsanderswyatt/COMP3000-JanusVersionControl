using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobExpressions;
using Janus.Plugins;

namespace Janus.Helpers
{
    public static class IgnoreHelper
    {
        public static (List<Glob> includePatterns, List<Glob> excludePatterns) LoadIgnorePatterns(string workingDir)
        {
            // Load .janusignore file
            var ignoreFilePath = Path.Combine(workingDir, ".janusignore");
            var ignorePatterns = File.Exists(ignoreFilePath) ? File.ReadAllLines(ignoreFilePath) : Array.Empty<string>();


            // Get include and exclude patterns
            var excludePatterns = new List<Glob>();
            var includePatterns = new List<Glob>();

            foreach (var pattern in ignorePatterns)
            {
                if (string.IsNullOrWhiteSpace(pattern) || pattern.StartsWith("#"))
                    continue; // Skip empty lines or comments

                if (pattern.StartsWith("!"))
                    includePatterns.Add(new Glob(pattern[1..], GlobOptions.Compiled));
                else
                    excludePatterns.Add(new Glob(pattern, GlobOptions.Compiled));
            }

            return (includePatterns, excludePatterns);
        }

        public static bool ShouldIgnore(string relPath, List<Glob> includePatterns, List<Glob> excludePatterns)
        {
            return excludePatterns.Any(pattern => pattern.IsMatch(relPath)) &&
                   !includePatterns.Any(pattern => pattern.IsMatch(relPath));
        }
        



    }
}
