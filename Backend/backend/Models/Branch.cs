using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace backend.Models
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }

        [Required]
        public int RepoId { get; set; }

        [Required]
        [StringLength(100)] // TODO: Determain branch length constraint
        public string BranchName { get; set; }

        [Required]
        public int CommitId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        
        public Repository Repository { get; set; }
        public Commit Commit { get; set; }

    }

}
