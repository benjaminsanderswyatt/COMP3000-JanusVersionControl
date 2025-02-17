namespace Janus.Models
{
    public class RepoData
    {
        public string RepoName { get; set; }
        public string RepoDescription { get; set; }
        public bool IsPrivate { get; set; }
        public List<Branch> Branches { get; set; }
    }
}
