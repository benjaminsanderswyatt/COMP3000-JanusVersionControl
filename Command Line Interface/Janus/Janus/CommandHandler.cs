using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Janus
{
    public static class CommandHandler
    {
        public static void Init(string[] args)
        {
            // Initialise .janus folder
            if (!Directory.Exists(Paths.janusDir))
            {
                Directory.CreateDirectory(Paths.objectDir);
                File.SetAttributes(Paths.janusDir, File.GetAttributes(Paths.janusDir) | FileAttributes.Hidden); // Makes the janus folder hidden

                Console.WriteLine("Initialized janus repository");
            }
            else
            {
                Console.WriteLine("Repository already initialized");
            }
        }

        public static void Add(string[] args)
        {
            string fileName = args[0];

            string blobHash = CommandHelper.SaveBlob(fileName);

            // Add the file and its hash to the staged area
            string stagedPath = Path.Combine(Paths.janusDir, "index");
            File.AppendAllText(stagedPath, $"{fileName} {blobHash}\n");


            Console.WriteLine($"Added {fileName} (blob {blobHash}).");
        }

        public static void Commit(string[] args)
        {
            string message = args[0];

            // Get files from staging area
            string stagedPath = Path.Combine(Paths.janusDir, "index");
            var files = new Dictionary<string, string>();

            foreach (var line in File.ReadLines(stagedPath))
            {
                var parts = line.Split(' ');
                files[parts[0]] = parts[1];
            }

            string treeHash = CommandHelper.SaveTree(files);
            string commitHash = CommandHelper.SaveCommit(treeHash, message);

            // Clear staging area
            File.WriteAllText(stagedPath, string.Empty);

            Console.WriteLine($"Committed with hash {commitHash}");
        }

        public static void CreateBranch(string[] args)
        {
            string branchName = args[0];
            string branchPath = Path.Combine(Paths.janusDir, "refs", branchName);

            // Put the latest commit into new branch
            string headPath = Path.Combine(Paths.janusDir, "HEAD");
            string latestCommit = File.ReadAllText(headPath);

            File.WriteAllText(branchPath, latestCommit);
            Console.WriteLine($"Created new branch {branchName}");
        }   

        public static void Log(string[] args)
        {
            foreach (var commitFile in Directory.GetFiles(Paths.objectDir))
            {
                string content = File.ReadAllText(commitFile);
                if (content.StartsWith("Tree:"))
                {
                    Console.WriteLine(content);
                }
            }
        }

    }
}
