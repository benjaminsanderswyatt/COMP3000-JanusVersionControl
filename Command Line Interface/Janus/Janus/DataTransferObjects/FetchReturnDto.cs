namespace Janus.DataTransferObjects
{
    public class FetchReturnDto
    {
        public RepoDataDto RepoData { get; set; }
        public Dictionary<string, string> BranchLatestHashes { get; set; }
        public List<BranchDto> NewBranches { get; set; }
        public Dictionary<string, List<CommitDto>> NewCommits { get; set; }
    }

    public class RepoDataDto
    {
        public string RepoDescription { get; set; }
        public bool IsPrivate { get; set; }
    }

}
