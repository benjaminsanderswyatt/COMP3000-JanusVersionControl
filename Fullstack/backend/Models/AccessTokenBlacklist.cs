using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class AccessTokenBlacklist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public DateTimeOffset Expires { get; set; }

        public DateTimeOffset BlacklistedAt { get; set; } = DateTimeOffset.UtcNow;

    }

}
