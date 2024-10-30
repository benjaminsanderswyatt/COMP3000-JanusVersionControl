
namespace Janus
{
    public class Metadata
    {
        public Dictionary<string, FileMetadata> Files { get; set; } = new Dictionary<string, FileMetadata>();
    }

    public class FileMetadata
    {
        public string LastCommitHash { get; set; }
        public string? DeltaHeadHash { get; set; }
    }

    public class DeltaNode
    {
        public string Content { get; set; }
        public string? NextDeltaHash { get; set; }
    }

}
