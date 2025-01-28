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
        public string FilePath { get; set; }

        [Required]
        public string FileHash { get; set; }



        public Commit Commit { get; set; }
        public FileContent FileContents { get; set; }

    }

}
