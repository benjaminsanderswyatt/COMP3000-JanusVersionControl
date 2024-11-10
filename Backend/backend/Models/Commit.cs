using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Commit
    {
        [Key]
        public int CommitId { get; set; }

        [Required]
        public int RepoId { get; set; }

        [Required]
        [StringLength(64)] // TODO: Determain constraint for hash length
        public string CommitHash { get; set; }

        [Required]
        [StringLength(256)] // TODO: Determain constraint for message length
        public string Message { get; set; }

        [Required]
        public int AuthorId { get; set; }

        public int? ParentCommitId { get; set; }  // Null for initial commit

        [Required]
        public DateTime CreatedAt { get; set; }

        

        public Repository Repository { get; set; }
        public User Author { get; set; }
        public Commit ParentCommit { get; set; }
        public ICollection<File> Files { get; set; }
        public ICollection<Branch> Branches { get; set; }

    }

}
