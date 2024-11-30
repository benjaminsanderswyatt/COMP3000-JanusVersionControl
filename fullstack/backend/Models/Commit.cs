using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Commit
    {
        [Key]
        public int CommitId { get; set; }

        [ForeignKey("Branch")]
        public int BranchId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(64)] // TODO: Determain constraint for hash length
        public string CommitHash { get; set; }

        [MaxLength(512)] // TODO: Determain constraint for message length
        public string Message { get; set; }

        public int? ParentCommitId { get; set; }  // Null for initial commit

        public DateTime CommittedAt { get; set; }



        public User User { get; set; }
        public Branch Branch { get; set; }
        public ICollection<File> Files { get; set; }

    }

}
