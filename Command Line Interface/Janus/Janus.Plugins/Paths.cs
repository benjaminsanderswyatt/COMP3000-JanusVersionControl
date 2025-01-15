namespace Janus.Plugins
{
    public class Paths
    {
        public string JanusDir { get; }
        public string ObjectDir { get; }
        public string CommitDir { get; }
        public string RefsDir { get; }
        public string HeadsDir { get; }
        public string PluginsDir { get; }
        public string Index { get; }
        public string HEAD { get; }

        public Paths(string basePath)
        {
            JanusDir = Path.Combine(basePath, ".janus");
            ObjectDir = Path.Combine(JanusDir, "objects");
            CommitDir = Path.Combine(JanusDir, "commits");
            RefsDir = Path.Combine(JanusDir, "refs");
            HeadsDir = Path.Combine(RefsDir, "heads");
            PluginsDir = Path.Combine(JanusDir, ".plugins");
            Index = Path.Combine(JanusDir, "index");
            HEAD = Path.Combine(JanusDir, "HEAD");

        }
    }
}

