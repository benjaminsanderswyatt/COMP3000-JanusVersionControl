using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class File
    {
        [Key]
        public int FileId { get; set; }
        public int CommitId { get; set; }
        public string FilePath { get; set; }
        public string FileBlob { get; set; }
        public DateTime CreatedAt { get; set; }

        

        public Commit Commit { get; set; }

    }

}
