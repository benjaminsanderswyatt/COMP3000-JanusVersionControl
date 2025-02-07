using System.ComponentModel.DataAnnotations;

namespace backend.DataTransferObjects
{
    public class RepositoryDto
    {
        [Required]
        [StringLength(255, ErrorMessage = "Repository name cannot exceed 256 characters.")]
        public string RepoName { get; set; }

        [StringLength(511, ErrorMessage = "Repository description cannot exceed 512 characters.")]
        public string RepoDescription { get; set; }

        public bool IsPrivate { get; set; }

    }

}
