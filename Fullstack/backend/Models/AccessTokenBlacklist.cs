using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class AccessTokenBlacklist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TokenHash { get; set; }

        [Required]
        public DateTime Expires { get; set; }

        public DateTime BlacklistedAt { get; set; } = DateTime.UtcNow;

    }

}
