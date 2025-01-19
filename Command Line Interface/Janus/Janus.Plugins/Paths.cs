namespace Janus.Plugins
{
    public class Paths
    {
        public string JanusDir { get; }
        public string WorkingDir { get; }
        public string ObjectDir { get; }
        public string TreeDir { get; }
        public string CommitDir { get; }
        public string HeadsDir { get; }
        public string PluginsDir { get; }
        public string Index { get; }
        public string HEAD { get; }
        public string DETACHED_HEAD { get; }
        public string BranchesDir { get; }

        public Paths(string basePath)
        {
            WorkingDir = basePath;
            JanusDir = Path.Combine(basePath, ".janus");
            ObjectDir = Path.Combine(JanusDir, "objects");
            TreeDir = Path.Combine(JanusDir, "trees");
            CommitDir = Path.Combine(JanusDir, "commits");
            HeadsDir = Path.Combine(JanusDir, "heads");
            PluginsDir = Path.Combine(JanusDir, ".plugins");
            Index = Path.Combine(JanusDir, "index");
            HEAD = Path.Combine(JanusDir, "HEAD");
            DETACHED_HEAD = Path.Combine(JanusDir, "DETACHED_HEAD");
            BranchesDir = Path.Combine(JanusDir, "branches");

        }
    }
}

