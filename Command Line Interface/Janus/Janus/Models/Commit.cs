namespace Janus.Models
{
    public class CommitMetadata
    {
        public string Commit { get; set; }
        public string Parent { get; set; }
        public string Branch { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public string Tree { get; set; }

    }
}
