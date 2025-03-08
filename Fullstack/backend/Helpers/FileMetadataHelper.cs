using Microsoft.AspNetCore.StaticFiles;

namespace backend.Helpers
{
    public class FileMetadataHelper
    {
        public class FileMetadata
        {
            public string Hash { get; set; }
            public string MimeType { get; set; }
            public long Size { get; set; }
            public DateTimeOffset LastModified { get; set; }
        }





    }
}
