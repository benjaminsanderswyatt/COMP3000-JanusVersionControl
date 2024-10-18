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
                Directory.CreateDirectory(Paths.janusDir);
            } else
            {
                Console.WriteLine("Repository already initialized");
                return;
            }

            File.SetAttributes(Paths.janusDir, File.GetAttributes(Paths.janusDir) | FileAttributes.Hidden); // Makes the janus folder hidden

            // .janus/object folder
            if (!Directory.Exists(Paths.objectDir))
                Directory.CreateDirectory(Paths.objectDir);

            // .janus/refs
            if (!Directory.Exists(Paths.refsDir))
                Directory.CreateDirectory(Paths.refsDir);

            // .janus/refs/heads
            if (!Directory.Exists(Paths.headsDir))
                Directory.CreateDirectory(Paths.headsDir);


            // Create index file
            File.Create(Paths.index).Close();

            // Create empty main branch in refs/heads/
            File.WriteAllText(Path.Combine(Paths.headsDir, "main"), string.Empty);

            // Create HEAD file pointing at main branch
            File.WriteAllText(Paths.head, "ref: refs/heads/main");

            Console.WriteLine("Initialized janus repository");
   
        }

        public static void Add(string[] args)
        {
            string fileName = args[0];

            string blobHash = CommandHelper.SaveBlob(fileName);

            // Add the file and its hash to the staged area
            File.AppendAllText(Paths.index, $"{fileName} {blobHash}\n");


            Console.WriteLine($"Added {fileName} (blob {blobHash}).");
        }

        public static void Commit(string[] args)
        {
            try
            {
                string message = args[0];

                // Get files from staging area
                var files = new Dictionary<string, string>();

                foreach (var line in File.ReadLines(Paths.index))
                {
                    var parts = line.Split(' ');
                    files[parts[0]] = parts[1];
                }

                string treeHash = CommandHelper.SaveTree(files);
                string commitHash = CommandHelper.SaveCommit(treeHash, message);

                // Clear staging area (index)
                File.WriteAllText(Paths.index, string.Empty);

                // Write current commit hash to refs/heads/[current branch]
                string currentBranch = File.ReadAllText(Paths.head).Replace("ref: ", "").Trim();
                string branchPath = Path.Combine(Paths.janusDir, currentBranch);
                File.WriteAllText(branchPath, commitHash);

                Console.WriteLine($"Committed to {currentBranch} with hash {commitHash}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


           }

        public static void CreateBranch(string[] args)
        {
            string branchName = args[0];
            string branchPath = Path.Combine(Paths.headsDir, branchName);

            if (File.Exists(branchPath))
            {
                Console.WriteLine($"Branch '{branchName}' already exists.");
                return;
            }

            // Get the latest commit of the curent branch
            string headPath = Path.Combine(Paths.janusDir, "HEAD");
            string currentBranch = File.ReadAllText(headPath).Replace("ref: ", "").Trim();
            string currentBranchPath = Path.Combine(Paths.janusDir, currentBranch);

            // Write the latest commit hash into the new branch file
            string latestCommitHash = File.ReadAllText(currentBranchPath).Trim();
            File.WriteAllText(branchPath, latestCommitHash);


            Console.WriteLine($"Created new branch {branchName}");
        }

        public static void SwitchBranch(string[] args)
        {
            string branchName = args[0];
            string branchPath = Path.Combine(Paths.headsDir, branchName);

            if (File.Exists(branchPath))
            {
                // Update HEAD to point to the switched to branch
                File.WriteAllText(Path.Combine(Paths.janusDir, "HEAD"), $"ref: refs/heads/{branchName}");
                Console.WriteLine($"Switched to branch {branchName}.");
            }
            else
            {
                Console.WriteLine($"Branch {branchName} does not exist.");
            }
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
