namespace Janus
{
    internal class Paths
    {
        public static readonly string janusDir = ".janus";

        public static readonly string TokenDir = Path.Combine(janusDir, "token");

        public static readonly string objectDir = Path.Combine(janusDir, "objects");
        public static readonly string commitDir = Path.Combine(janusDir, "commits");
        public static readonly string refsDir = Path.Combine(janusDir, "refs");
        public static readonly string headsDir = Path.Combine(refsDir, "heads");
        public static readonly string pluginsDir = ".plugins";

        public static readonly string index = Path.Combine(janusDir, "index");
        public static readonly string head = Path.Combine(janusDir, "HEAD");
    }
}

