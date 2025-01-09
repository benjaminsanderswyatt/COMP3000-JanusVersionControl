namespace backend.DataTransferObjects
{
    public class CommitDto
    {
        public string BranchName { get; set; }
        public string CommitHash { get; set; }
        public string Message { get; set; }
        public string ParentCommitHash { get; set; }
        public DateTimeOffset CommittedAt { get; set; }
        public List<FileDto> Files { get; set; }
    }

    public class FileDto
    {
        public string FilePath { get; set; }
        public string FileHash { get; set; }
        public byte[] FileContent { get; set; }
    }

}
