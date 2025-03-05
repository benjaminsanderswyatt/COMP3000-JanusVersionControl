using GlobExpressions;
using Janus.Plugins;
using Janus.Utils;
using Microsoft.AspNetCore.StaticFiles;
using static Janus.Helpers.FileMetadataHelper;

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

        public static Dictionary<string, FileMetadata> GetWorkingDirFileHash(Paths paths)
        {
            var workingDirFiles = GetAllFilesInDir(paths, paths.WorkingDir);
            var filePathMeta = new Dictionary<string, FileMetadata>();

            foreach (var relPath in workingDirFiles)
            {
                var fullPath = Path.Combine(paths.WorkingDir, relPath);
                
                FileMetadata metadata = GetFileMetadata(fullPath, HashHelper.ComputeHashGivenRelFilepath(paths.WorkingDir, relPath));

                filePathMeta[relPath] = metadata;
            }

            return filePathMeta;
        }


    }
}
