namespace backend.DataTransferObjects.CLI
{
    public class CommitDto
    {
        public string CommitHash { get; set; }
        public string BranchName { get; set; }
        public string TreeHash { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public string Message { get; set; }
        public string ParentCommitHash { get; set; }
        public DateTime CommittedAt { get; set; }
    }

    public class FileDto
    {
        public string FilePath { get; set; }
        public string FileHash { get; set; }
        public byte[] FileContent { get; set; }
    }

}
