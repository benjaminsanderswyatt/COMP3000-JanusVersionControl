namespace backend.DataTransferObjects.CLI
{
    public class BranchDto
    {
        public string BranchName { get; set; }
        public string ParentBranch { get; set; }
        public string SplitFromCommitHash { get; set; }
        public string LatestCommitHash { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset Created { get; set; }
        public List<CommitDto> Commits { get; set; }
    }

}
