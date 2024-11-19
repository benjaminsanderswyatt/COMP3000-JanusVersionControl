using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace backend.Models
{
    public class FileContent
    {
        [Key]
        public int ContentId { get; set; }

        [ForeignKey("File")]
        public int FileId { get; set; }

        [ForeignKey("Commit")]
        public int CommitId { get; set; }

        [Required]
        public byte[] Content { get; set; }


        public File File { get; set; }
        public Commit Commit { get; set; }
    }

}
