using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class File
    {
        [Key]
        public int FileId { get; set; }

        [Required]
        public int CommitId { get; set; }

        [Required]
        [StringLength(512)] // TODO: Determine max length for file path
        public string FilePath { get; set; }

        [Required]
        public string FileBlob { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        

        public Commit Commit { get; set; }

    }

}
