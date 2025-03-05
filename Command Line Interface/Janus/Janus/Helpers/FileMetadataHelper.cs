using Janus.Helpers.CommandHelpers;
using Janus.Utils;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janus.Helpers
{
    public class FileMetadataHelper
    {
        private static readonly FileExtensionContentTypeProvider _mimeProvider = new();

        public class FileMetadata
        {
            public string Hash { get; set; }
            public string MimeType { get; set; }
            public long Size { get; set; }
        }



        public static FileMetadata GetFileMetadata(string fullPath, string hash)
        {
            var newMeta = new FileMetadata
            {
                Hash = hash,
                MimeType = GetMimeType(fullPath),
                Size = new FileInfo(fullPath).Length
            };

            return newMeta;
        }


        private static string GetMimeType(string fullPath)
        {
            // Get mime type
            if (!_mimeProvider.TryGetContentType(fullPath, out var mimeType))
            {
                mimeType = IsTextFile(fullPath) ? "text/plain" : "application/octet-stream";
            }

            return mimeType;
        }



        private static bool IsTextFile(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                var buffer = new byte[1024];
                var bytesRead = stream.Read(buffer);

                return !buffer.Take(bytesRead).Any(b => b == 0 || b > 127);
            }
            catch
            {
                return false;
            }
        }


        public static string FormatSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            var i = 0;
            double size = bytes;

            while (size >= 1024 && i < suffixes.Length - 1)
            {
                size /= 1024;
                i++;
            }

            return $"{size:0.##} {suffixes[i]}";
        }



    }
}
