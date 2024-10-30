using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

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

        public static string LoadBlob(string blobHash)
        {
            string blobPath = Path.Combine(Paths.objectDir, blobHash);

            if (File.Exists(blobPath))
            {
                return File.ReadAllText(blobPath);
            }

            return null;
        }


        public static string GetHash(string content)
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

        public static string SaveDeltaNode(DeltaNode deltaNode)
        {
            // Save delta to objects dir
            string deltaHash = GetHash(deltaNode.Content);
            string deltaPath = Path.Combine(Paths.objectDir, deltaHash);

            if (!File.Exists(deltaPath))
            {
                var json = JsonConvert.SerializeObject(deltaNode, Formatting.Indented);
                File.WriteAllText(deltaPath, json);
            }
            
            return deltaHash;
        }

        public static string GetDelta(string previousContent, string currentContent)
        {
            var differ = new Differ();
            var inlineBuilder = new InlineDiffBuilder(differ);

            var diffResult = inlineBuilder.BuildDiffModel(previousContent, currentContent);

            var deltaBuilder = new StringBuilder();

            foreach (var line in diffResult.Lines)
            {
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        deltaBuilder.AppendLine($"+ {line.Text}");
                        break;
                    case ChangeType.Deleted:
                        deltaBuilder.AppendLine($"- {line.Text}");
                        break;
                    case ChangeType.Modified:
                        deltaBuilder.AppendLine($"~ {line.Text}");
                        break;
                    case ChangeType.Unchanged:
                        deltaBuilder.AppendLine($"  {line.Text}");
                        break;
                }
            }

            return deltaBuilder.ToString();
        }

        public static string ReconstructFile(string fileName)
        {
            var metadata = LoadMetadata();
            if (!metadata.Files.ContainsKey(fileName))
            {
                return null;
            }

            var fileMetadata = metadata.Files[fileName];
            string content = LoadBlob(fileMetadata.LastCommitHash);

            string? currentDeltaHash = fileMetadata.DeltaHeadHash;

            while (currentDeltaHash != null)
            {
                var deltaNode = LoadDeltaNode(currentDeltaHash);
                content = GetDelta(content, deltaNode.Content);
                currentDeltaHash = deltaNode.NextDeltaHash;
            }

            return content;
        }


        public static DeltaNode LoadDeltaNode(string deltaHash)
        {
            string deltapath = Path.Combine(Paths.objectDir, deltaHash);

            if (File.Exists(deltapath))
            {
                var json = File.ReadAllText(deltapath);
                return JsonConvert.DeserializeObject<DeltaNode>(json);
            }

            return null;
        }


        public static Metadata LoadMetadata()
        {
            string metadataFile = Path.Combine(Paths.janusDir, "metadata.json");
            if (File.Exists(metadataFile))
            {
                var json = File.ReadAllText(metadataFile);
                return JsonConvert.DeserializeObject<Metadata>(json);
            }
            return new Metadata();
        }

        public static void SaveMetadata(Metadata metadata)
        {
            string metadataFile = Path.Combine(Paths.janusDir, "metadata.json");

            var json = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            
            File.WriteAllText(metadataFile, json);
        }



    }
}
