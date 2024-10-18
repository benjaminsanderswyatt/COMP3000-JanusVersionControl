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

        public static string SaveTree(Dictionary<string, string> files)
        {
            StringBuilder treeContent = new StringBuilder();

            foreach (var file in files)
            {
                string fileName = file.Key;
                string blobHash = file.Value;
                treeContent.AppendLine($"{fileName} {blobHash}");
            }

            string treeHash = GetHash(treeContent.ToString());
            string treePath = Path.Combine(Paths.objectDir, treeHash);

            File.WriteAllText(treePath, treeContent.ToString());

            return treeHash;
        }


        public static string SaveCommit(string treeHash, string message)
        {
            string commitContent = $"Tree: {treeHash}\nMessage: {message}\nTimestamp: {DateTime.Now}\n";
            string commitHash = GetHash(commitContent);

            string commitPath = Path.Combine(Paths.objectDir, commitHash);
            File.WriteAllText(commitPath, commitContent);

            return commitHash;
        }


    }
}
