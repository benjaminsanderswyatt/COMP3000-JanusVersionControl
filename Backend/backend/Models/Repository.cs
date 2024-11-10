using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Repository
    {
        [Key]
        public int RepoId { get; set; }
        public int UserId { get; set; }
        public int RepoName { get; set; }
        public bool Visibility { get; set; }
        public DateTime CreatedAt { get; set; }

        

        public User User { get; set; }
        public ICollection<Branch> Branches { get; set; }
        public ICollection<Commit> Commits { get; set; }
    }

}
