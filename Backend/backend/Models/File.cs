using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class File
    {
        [Key]
        public int FileId { get; set; }

        [ForeignKey("Commit")]
        public int CommitId { get; set; }

        [Required]
        [MaxLength(512)] // TODO: Determine max length for file path
        public string Path { get; set; }

        [Required]
        [MaxLength(256)]
        public string FileName { get; set; }

        [Required]
        public string FileHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        

        public Commit Commit { get; set; }
        public ICollection<FileContent> FileContents { get; set; }

    }

}
