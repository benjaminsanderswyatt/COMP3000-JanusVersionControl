using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Commit
    {
        [Key]
        public int CommitId { get; set; }
        public int RepoId { get; set; }
        public string CommitHash { get; set; }
        public string Message { get; set; }
        public int AuthorId { get; set; }
        public int? ParentCommitId { get; set; }  // Null for initial commit
        public DateTime CreatedAt { get; set; }

        

        public Repository Repository { get; set; }
        public User Author { get; set; }
        public Commit ParentCommit { get; set; }
        public ICollection<File> Files { get; set; }
        public ICollection<Branch> Branches { get; set; }

    }

}
