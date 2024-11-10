using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }

        

        public ICollection<Repository> Repositories { get; set; }
        public ICollection<Commit> Commits { get; set; }

    }

}
