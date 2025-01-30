using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Tree
    {
        [Key]
        [StringLength(40)]
        public int TreeHash { get; set; }

        [Required]
        public string EntryName { get; set; }

        [Required]
        public string EntryType { get; set; } = "blob";

        [Required]
        [StringLength(40)]
        public string EntryHash { get; set; }


    }

}
