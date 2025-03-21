using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Repository
    {
        [Key]
        public int RepoId { get; set; }

        [Required]
        public int OwnerId { get; set; }

        [Required]
        [StringLength(255)]
        public string RepoName { get; set; }

        [StringLength(511)]
        public string RepoDescription { get; set; } = string.Empty;

        [Required]
        public bool IsPrivate { get; set; } = true; // true - > hidden. false -> visable

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;


        [ForeignKey("OwnerId")]
        public User Owner { get; set; } = null;

        public ICollection<RepoAccess> RepoAccesses { get; set; } = new List<RepoAccess>();
        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public ICollection<RepoInvite> RepoInvites { get; set; } = new List<RepoInvite>();
    }

}
