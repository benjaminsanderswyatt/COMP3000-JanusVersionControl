namespace Janus.DataTransferObjects
{

    public class CloneDto
    {
        public string RepoName { get; set; }
        public string RepoDescription { get; set; }
        public bool IsPrivate { get; set; }
        public List<BranchDto> Branches { get; set; }

    }

    public class BranchDto
    {
        public string BranchName { get; set; }
        public int? ParentBranch { get; set; }
        public string SplitFromCommitHash { get; set; }
        public string LatestCommitHash { get; set; }
        public string CreatedBy { get; set; }
        public List<CommitDto> Commits { get; set; }
    }

    public class CommitDto
    {
        public string CommitHash { get; set; }
        public string ParentCommitHash { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public string Message { get; set; }
        public DateTime CommittedAt { get; set; }
        public string TreeHash { get; set; }
        public TreeDto? Tree { get; set; }
    }


    public class TreeDto
    {
        public string Name { get; set; } // File or folder name
        public string? Hash { get; set; } // null if folder
        public List<TreeDto> Children { get; set; }
    }


}
