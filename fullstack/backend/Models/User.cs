using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(64)] // TODO: Determain string length constraint
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public byte[] Salt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;



        public ICollection<Repository> Repositories { get; set; }
        public ICollection<Collaborator> Collaborators { get; set; }

    }

}
