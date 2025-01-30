using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class File
    {
        [Key]
        [StringLength(40)]
        public string FileHash { get; set; }

        public int Size { get; set; }

        [Required]
        public string StoragePath { get; set; } // Stored in external

    }

}
