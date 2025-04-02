namespace backend.DataTransferObjects.CLI
{
    public class CloneDto
    {
        public string RepoName { get; set; }
        public string RepoDescription { get; set; }
        public bool IsPrivate { get; set; }
        public List<BranchDto> Branches { get; set; }

    }

}
