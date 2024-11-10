using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }
        public int RepoId { get; set; }
        public string BranchName { get; set; }
        public int CommitId { get; set; }
        public DateTime CreatedAt { get; set; }

        
        public Repository Repository { get; set; }
        public Commit Commit { get; set; }

    }

}
