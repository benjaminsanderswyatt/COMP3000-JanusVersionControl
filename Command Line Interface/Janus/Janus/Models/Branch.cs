namespace Janus.Models
{
    public class Branch
    {
        public string Name { get; set; }
        public string InitialCommit { get; set; }
        public string? CreatedBy { get; set; }
        public string? ParentBranch { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}
