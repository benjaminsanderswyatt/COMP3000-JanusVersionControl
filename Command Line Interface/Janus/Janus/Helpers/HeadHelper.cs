
namespace Janus
{
    internal class HeadHelper
    {
        public static void SetHeadCommit(string commitHash)
        {
            var refHead = File.ReadAllText(Paths.head).Substring(5); // Remove the "ref: " at the start
            File.WriteAllText(Path.Combine(Paths.janusDir, refHead), commitHash);
        }



    }
}
