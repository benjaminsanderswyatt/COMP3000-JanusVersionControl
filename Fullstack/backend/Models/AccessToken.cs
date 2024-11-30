using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class AccessToken
    {
        [Key]
        public int TokenId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public string TokenHash { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime Expires { get; set; } = DateTime.UtcNow.AddMonths(3); // Expires x months


        public User User { get; set; }

    }

}
