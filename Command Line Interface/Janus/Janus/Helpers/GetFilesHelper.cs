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
    public static class GetFilesHelper
    {

        public static IEnumerable<string> GetAllFilesInDir(Paths paths, string directory)
        {
            // Load ignore patterns
            var (includePatterns, excludePatterns) = IgnoreHelper.LoadIgnorePatterns(paths.WorkingDir);

            // Get all files in directory
            var directoryFiles = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                                       .Select(filePath => Path.GetRelativePath(paths.WorkingDir, filePath))
                                       .Where(path => !path.StartsWith(".janus" + Path.DirectorySeparatorChar)) // Always exclude .janus directory
                                       .Where(path => !MatchesAnyGlob(path, excludePatterns) || MatchesAnyGlob(path, includePatterns));

            return directoryFiles;
        }

        private static bool MatchesAnyGlob(string path, IEnumerable<Glob> patterns)
        {
            return patterns.Any(pattern => pattern.IsMatch(path));
        }

        



    }
}
