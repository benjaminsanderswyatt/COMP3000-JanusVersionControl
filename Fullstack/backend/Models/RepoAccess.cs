using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace backend.Models
{
    public class RepoAccess
    {
        [Required]
        public int RepoId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public AccessLevel AccessLevel { get; set; }


        [ForeignKey("RepoId")]
        public Repository Repository { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }


    public enum AccessLevel
    {
        READ,
        WRITE,
        ADMIN
    }
}
