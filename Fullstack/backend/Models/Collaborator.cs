using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Collaborator
    {
        [Key]
        public int RepoId { get; set; }

        [Key]
        public int UserId { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;



        public Repository Repository { get; set; }
        public User User { get; set; }

    }

}
