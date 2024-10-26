using Janus.Plugins;

namespace Janus
{
    public static class CommandHandler
    {
        public static List<ICommand> GetCommands()
        {
            return new List<ICommand>
            {
                new InitCommand(),
                new AddCommand(),
                new CommitCommand(),
                new CreateBranchCommand(),
                new SwitchBranchCommand(),
                new LogCommand()

                // Add new built in commands here
            };
        }


        public class InitCommand : ICommand
        {
            public string Name => "init";
            public string Description => "Initializes the repository.";
            public void Execute(string[] args)
            {
                // Initialise .janus folder
                if (!Directory.Exists(Paths.janusDir))
                {
                    Directory.CreateDirectory(Paths.janusDir);
                }
                else
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

                // .janus/plugins folder
                if (!Directory.Exists(Paths.pluginsDir))
                    Directory.CreateDirectory(Paths.pluginsDir);


                // Create index file
                File.Create(Paths.index).Close();

                // Create empty main branch in refs/heads/
                File.WriteAllText(Path.Combine(Paths.headsDir, "main"), string.Empty);

                // Create HEAD file pointing at main branch
                File.WriteAllText(Paths.head, "ref: refs/heads/main");

                Console.WriteLine("Initialized janus repository");
            }
        }

        public class AddCommand : ICommand
        {
            public string Name => "add";
            public string Description => "add help";
            public void Execute(string[] args)
            {
                string fileName = args[0];

                string blobHash = CommandHelper.SaveBlob(fileName);

                // Add the file and its hash to the staged area
                File.AppendAllText(Paths.index, $"{fileName} {blobHash}\n");


                Console.WriteLine($"Added {fileName} (blob {blobHash}).");
            }
        }

        public class CommitCommand : ICommand
        {
            public string Name => "commit";
            public string Description => "commit";
            public void Execute(string[] args)
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
        }

        public class CreateBranchCommand : ICommand
        {
            public string Name => "create branch";
            public string Description => "create branch help";
            public void Execute(string[] args)
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
        }

        public class SwitchBranchCommand : ICommand
        {
            public string Name => "switch branch";
            public string Description => "switch branch help";
            public void Execute(string[] args)
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
        }

        public class LogCommand : ICommand
        {
            public string Name => "log";
            public string Description => "log help";
            public void Execute(string[] args)
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
}
