using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)] // TODO: Determain string length constraint
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [StringLength(256)]
        public string Password { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        

        public ICollection<Repository> Repositories { get; set; }
        public ICollection<Commit> Commits { get; set; }

    }

}
