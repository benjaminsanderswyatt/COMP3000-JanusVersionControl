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

        public string? LatestCommitHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;


        [ForeignKey("RepoId")]
        public Repository Repository { get; set; }

    }

}
