using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }

        [Required]
        [MaxLength(255)]
        public string BranchName { get; set; }

        [Required]
        public int RepoId { get; set; }

        [MaxLength(40)]
        public string? SplitFromCommitHash { get; set; } = null;

        [MaxLength(40)]
        public string? LatestCommitHash { get; set; } = null;

        public int CreatedBy { get; set; }

        public int? ParentBranch { get; set; } // root branch is null (has no parent)

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;


        [ForeignKey("RepoId")]
        public Repository Repository { get; set; }

        public ICollection<Commit> Commits { get; set; } = new List<Commit>();

        [ForeignKey("ParentBranch")]
        public Branch Parent { get; set; }

    }

}
