namespace backend.DataTransferObjects.CLI
{
    public class PushDto
    {
        public List<CommitDto> Commits { get; set; }
        public List<TreeDto> Trees { get; set; }
        public List<FileDto> Files { get; set; }
    }

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

    public class TreeDto
    {
        
    }

    public class FileDto
    {
        public string FilePath { get; set; }
        public byte[] FileContent { get; set; }
    }

}
