namespace backend.DataTransferObjects.CLI
{
    public class TreeDto
    {
        public string Name { get; set; } // File or folder name
        public string? Hash { get; set; } // null if folder
        public string MimeType { get; set; }
        public long Size { get; set; }
        public DateTimeOffset LastModified { get; set; }
        public List<TreeDto> Children { get; set; }
    }

}
