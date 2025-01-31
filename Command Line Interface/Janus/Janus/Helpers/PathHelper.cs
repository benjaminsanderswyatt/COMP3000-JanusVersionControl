namespace Janus.Helpers
{
    public static class PathHelper
    {

        public static string[] PathSplitter(string path)
        {
            return path.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
        }


    }
}
