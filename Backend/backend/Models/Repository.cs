using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Repository
    {
        [Key]
        public int RepoId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)] // TODO: Determain string length constraint
        public string RepoName { get; set; }

        [Required]
        public bool Visibility { get; set; } = true; // true - > visable. false -> hidden
        
        [Required]
        public DateTime CreatedAt { get; set; }

        

        public User User { get; set; }
        public ICollection<Branch> Branches { get; set; }
        public ICollection<Commit> Commits { get; set; }
    }

}
