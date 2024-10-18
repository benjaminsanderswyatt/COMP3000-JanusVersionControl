using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Janus
{
    internal class CommandHelper
    {
        public static string SaveBlob(string filePath)
        {
            string content = File.ReadAllText(filePath);
            string hash = GetHash(content);
            string objectPath = Path.Combine(Paths.objectDir, hash);

            File.WriteAllText(objectPath, content);
            return hash;
        }

        private static string GetHash(string content)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                byte[] hashBytes = sha1.ComputeHash(contentBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "");
            }
        }

    }
}
