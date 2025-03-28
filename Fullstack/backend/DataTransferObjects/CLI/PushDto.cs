namespace backend.DataTransferObjects.CLI
{
    public class PushDto
    {
        public string RepoOwner { get; set; }
        public string RepoName { get; set; }
        public string BranchName { get; set; }
        public List<CommitDto> Commits { get; set; }
        public List<string> NewFileHashes { get; set; }
    }

    public class CommitDto
    {
        public string CommitHash { get; set; }
        public List<string> ParentHashes { get; set; }
        public string TreeHash { get; set; }
        public string Message { get; set; }
        public DateTimeOffset CommittedAt { get; set; }
    }

}
