using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class CommitMetadata
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string CommitHash { get; set; } // FK

        [Required]
        [MaxLength(255)]
        public string AuthorName { get; set; }

        [Required]
        [MaxLength(255)]
        public string AuthorEmail { get; set; }

        [Required]
        public DateTime CommittedAt { get; set; } = DateTime.UtcNow;

        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }

}
