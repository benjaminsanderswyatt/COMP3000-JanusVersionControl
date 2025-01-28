using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class FileContent
    {
        [Key]
        public int FileId { get; set; }

        [Required]
        public byte[] Content { get; set; }


        public File File { get; set; }
    }

}
