using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Commit
    {
        [Key]
        [StringLength(40)]
        public string CommitHash { get; set; }

        [ForeignKey("Branch")]
        public int BranchId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }


        [MaxLength(256)]
        public string Message { get; set; }

        public int? ParentCommitId { get; set; }  // Null for initial commit

        public DateTimeOffset CommittedAt { get; set; }



        public User User { get; set; }
        public Branch Branch { get; set; }
        public ICollection<File> Files { get; set; }

    }

}
