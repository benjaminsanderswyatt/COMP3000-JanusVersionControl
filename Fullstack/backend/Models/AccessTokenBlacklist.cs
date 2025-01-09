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
        public DateTime Expires { get; set; }

        public DateTime BlacklistedAt { get; set; } = DateTime.UtcNow;

    }

}
