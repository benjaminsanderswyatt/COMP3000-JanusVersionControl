using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Repository
    {
        [Key]
        public int RepoId { get; set; }

        [ForeignKey("User")]
        public int OwnerId { get; set; }

        [Required]
        [StringLength(256)] // TODO: Determain string length constraint
        public string RepoName { get; set; }

        [Required]
        public bool Visibility { get; set; } = false; // true - > visable. false -> hidden

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



        public User Owner { get; set; }
        public ICollection<Branch> Branches { get; set; }
        public ICollection<Collaborator> Collaborators { get; set; }
    }

}
