using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(64)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MaxLength(64)]
        public string PasswordHash { get; set; }

        [Required]
        public byte[] Salt { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public string? RefreshToken { get; set; }

        public DateTimeOffset? RefreshTokenExpiryTime { get; set; }

        public string? ProfilePicturePath { get; set; } = null;


        public ICollection<Repository> Repositories { get; set; } = new List<Repository>();
        public ICollection<RepoAccess> RepoAccesses { get; set; } = new List<RepoAccess>();
        public ICollection<RepoInvite> SentInvites { get; set; } = new List<RepoInvite>();
        public ICollection<RepoInvite> ReceivedInvites { get; set; } = new List<RepoInvite>();
    }

}
