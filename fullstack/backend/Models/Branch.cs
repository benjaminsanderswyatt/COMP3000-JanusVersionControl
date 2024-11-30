using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }

        [ForeignKey("Repository")]
        public int RepoId { get; set; }

        [Required]
        [MaxLength(256)] // TODO: Determain branch length constraint
        public string BranchName { get; set; }

        public int? LatestCommitId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public Repository Repository { get; set; }
        public ICollection<Commit> Commits { get; set; }

    }

}
