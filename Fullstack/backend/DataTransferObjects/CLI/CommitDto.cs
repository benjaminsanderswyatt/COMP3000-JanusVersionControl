namespace backend.DataTransferObjects.CLI
{
    public class CommitDto
    {
        public string CommitHash { get; set; }
        public List<string> ParentsCommitHash { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public string Message { get; set; }
        public DateTimeOffset CommittedAt { get; set; }
        public string TreeHash { get; set; }
        public TreeDto? Tree { get; set; }
    }

}
