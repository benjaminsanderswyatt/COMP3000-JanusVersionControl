using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class RepoInvite
    {
        [Key]
        public int InviteId { get; set; }

        [Required]
        public int RepoId { get; set; }

        [Required]
        public int InviterUserId { get; set; }

        [Required]
        public int InviteeUserId { get; set; }

        [Required]
        public AccessLevel AccessLevel { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;


        [ForeignKey("RepoId")]
        public Repository Repository { get; set; }

        [ForeignKey("InviterUserId")]
        public User Inviter { get; set; }

        [ForeignKey("InviteeUserId")]
        public User Invitee { get; set; }
    }


}
