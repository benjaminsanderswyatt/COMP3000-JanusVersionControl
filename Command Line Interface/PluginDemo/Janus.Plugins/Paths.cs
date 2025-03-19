using System.Runtime.InteropServices;

namespace Janus.Plugins
{
    public class Paths
    {
        public string JanusDir { get; }
        public string WorkingDir { get; }
        public string ObjectDir { get; }
        public string TreeDir { get; }
        public string CommitDir { get; }
        public string PluginsDir { get; }
        public string GlobalPluginsDir { get; }
        public string Index { get; }
        public string HEAD { get; }
        public string DETACHED_HEAD { get; }
        public string BranchesDir { get; }

        public string Remote { get; }
        public string RemoteCommitDir { get; }

        public string RepoConfig { get; }

        public string GlobalConfig { get; }

        public Paths(string basePath)
        {
            WorkingDir = basePath;
            JanusDir = Path.Combine(basePath, ".janus");
            ObjectDir = Path.Combine(JanusDir, "objects");
            TreeDir = Path.Combine(JanusDir, "trees");
            CommitDir = Path.Combine(JanusDir, "commits");
            PluginsDir = Path.Combine(JanusDir, "plugins");
            GlobalPluginsDir = GetGlobalPluginPath();
            Index = Path.Combine(JanusDir, "index");
            HEAD = Path.Combine(JanusDir, "HEAD");
            DETACHED_HEAD = Path.Combine(JanusDir, "DETACHED_HEAD");
            BranchesDir = Path.Combine(JanusDir, "branches");

            Remote = Path.Combine(JanusDir, "remote");
            RemoteCommitDir = Path.Combine(JanusDir, "remote");

            RepoConfig = Path.Combine(JanusDir, "repoConfig.json");

            GlobalConfig = GetGlobalConfigPath();
        }


        public static string GetGlobalPluginPath()
        {
            string globalPluginPath = Path.Combine(GetGlobalJanusPath(), "plugins");

            if (!Directory.Exists(globalPluginPath))
            {
                Directory.CreateDirectory(globalPluginPath);
            }

            return globalPluginPath;
        }

        public static string GetGlobalConfigPath()
        {
            string configPath = Path.Combine(GetGlobalJanusPath(), "config");

            if (!Directory.Exists(configPath))
            {
                Directory.CreateDirectory(configPath);
            }

            return configPath;
        }



        public static string GetGlobalJanusPath()
        {
            string basePath;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows: use %APPDATA%
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // MacOS: use /Library/Application Support
                string home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                basePath = Path.Combine(home, "Library", "Application Support");
            }
            else
            {
                // Misc case
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }


            string globalPath = Path.Combine(basePath, "Janus");

            if (File.Exists(globalPath))
            {
                File.Delete(globalPath);
            }

            if (!Directory.Exists(globalPath))
            {
                Directory.CreateDirectory(globalPath);
            }


            return globalPath;
        }
    }
}

