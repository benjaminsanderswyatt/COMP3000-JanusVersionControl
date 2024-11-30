using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class FileContent
    {
        [Key]
        public int ContentId { get; set; }

        [ForeignKey("File")]
        public int FileId { get; set; }

        [Required]
        public byte[] Content { get; set; }


        public File File { get; set; }
    }

}
