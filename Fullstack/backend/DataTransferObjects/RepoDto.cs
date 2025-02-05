using System.ComponentModel.DataAnnotations;

namespace backend.DataTransferObjects
{
    public class RepositoryDto
    {
        [Required]
        [StringLength(256, ErrorMessage = "Repository name cannot exceed 256 characters.")]
        public string RepoName { get; set; }

        public bool IsPrivate { get; set; }

    }

}
