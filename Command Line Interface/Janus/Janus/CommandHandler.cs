using Janus.Helpers;
using Janus.Plugins;
using System.Text;


namespace Janus
{
    public static class CommandHandler
    {
        public static List<ICommand> GetCommands(ILogger logger)
        {
            var commands = new List<ICommand>
            {
                new TestCommand(),
                new HelpCommand(),
                new InitCommand(),
                new AddCommand(),
                new CommitCommand(),
                //new PushCommand(),
                //new CreateBranchCommand(),
                //new SwitchBranchCommand(),
                //new LogCommand()
                
                // Add new built in commands here
            };

            // Pass the logger to each command
            foreach (var command in commands)
            {
                command.SetLogger(logger);
            }


            return commands;
        }

        public class TestCommand : BaseCommand
        {
            public override string Name => "test";
            public override string Description => "Send a test request to backend";
            public override void Execute(string[] args)
            {
                //await CommandHelper.ExecuteAsync();
                Logger.Log("Test End");
               
            }
        }


        public class HelpCommand : BaseCommand
        {
            public override string Name => "help";
            public override string Description => "Displays a list of available commands.";
            public override void Execute(string[] args)
            {
                Logger.Log("Usage: janus <command>");
                Logger.Log("Commands:");
                foreach (var command in Program.CommandList)
                {
                    Logger.Log($"{command.Name.PadRight(20)} : {command.Description}");
                }
            }
        }


        public class InitCommand : BaseCommand
        {
            public override string Name => "init";
            public override string Description => "Initializes the janus repository.";
            public override void Execute(string[] args)
            {
                try
                {
                    // Initialise .janus folder
                    if (!Directory.Exists(Paths.janusDir))
                    {
                        Directory.CreateDirectory(Paths.janusDir);
                        File.SetAttributes(Paths.janusDir, File.GetAttributes(Paths.janusDir) | FileAttributes.Hidden); // Makes the janus folder hidden
                    }
                    else
                    {
                        Logger.Log("Repository already initialized");
                        return;
                    }

                    Directory.CreateDirectory(Paths.objectDir); // .janus/object folder
                    Directory.CreateDirectory(Paths.refsDir); // .janus/refs
                    Directory.CreateDirectory(Paths.headsDir); // .janus/refs/heads
                    Directory.CreateDirectory(Paths.pluginsDir); // .janus/plugins folder


                    // Create index file
                    File.Create(Paths.index).Close();

                    // Create empty main branch in refs/heads/
                    File.WriteAllText(Path.Combine(Paths.headsDir, "main"), string.Empty);

                    // Create HEAD file pointing at main branch
                    File.WriteAllText(Paths.head, "ref: refs/heads/main");

                    Logger.Log("Initialized janus repository");


                }
                catch (Exception ex)
                {
                    Logger.Log($"Error initializing repository: {ex.Message}");
                }
            }
        }



        public class AddCommand : BaseCommand
        {
            public override string Name => "add";
            public override string Description => "Adds files to the staging area. To add all files use 'janus add all'.";
            public override void Execute(string[] args)
            {
                // No arguments given so command should return error
                if (args.Length < 1)
                {
                    Logger.Log("No files specified.");
                    return;
                }

                // Repository has to be initialised for command to run
                if (!Directory.Exists(Paths.janusDir))
                {
                    Logger.Log("Not a janus repository. Run 'janus init' first.");
                    return;
                }


                var filesToAdd = new List<string>();

                // When the first arg is 'all' ignore other arguments and stage all files
                if (args[0].Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    filesToAdd = Directory.EnumerateFiles(".", "*", SearchOption.AllDirectories)
                                  .Select(filePath => Path.GetRelativePath(".", filePath))
                                  .Where(relativePath => !relativePath.StartsWith(".janus"))
                                  .ToList();
                }
                else
                {
                    filesToAdd.AddRange(args);
                }


                // Index holds all of the files which have been added before
                /*
                HashSet<string> stagedFiles = new HashSet<string>();
                if (File.Exists(Paths.index))
                {
                    foreach (var line in File.ReadAllLines(Paths.index))
                    {
                        stagedFiles.Add(line.Trim());
                    }
                }
                */
                var stagedFiles = new Dictionary<string, string>();
                if (File.Exists(Paths.index))
                {
                    foreach (var line in File.ReadAllLines(Paths.index))
                    {
                        var parts = line.Split('|');
                        if (parts.Length == 2)
                            stagedFiles[parts[0].Trim()] = parts[1].Trim();
                    }
                }

                // Stage each file
                foreach (string relativeFilePath in filesToAdd)
                {
                    // File doesnt exist so it returns error and continues staging other files
                    if (!File.Exists(relativeFilePath))
                    {
                        Logger.Log($"File '{relativeFilePath}' not found.");
                        continue;
                    }

                    // Compute file hash
                    string fileHash = CommandHelper.ComputeHash(File.ReadAllText(relativeFilePath));

                    // Normalize paths for comparison
                    if (!stagedFiles.ContainsKey(relativeFilePath) || stagedFiles[relativeFilePath] != fileHash)
                    {
                        stagedFiles[relativeFilePath] = fileHash;
                        Logger.Log($"Added '{relativeFilePath}' to the staging area.");
                    }
                    else
                    {
                        Logger.Log($"File '{relativeFilePath}' is already staged.");
                    }


                }

                // Update index
                File.WriteAllLines(Paths.index, stagedFiles.Select(kv => $"{kv.Key}|{kv.Value}"));

            }
        }








        public class CommitCommand : BaseCommand
        {
            public override string Name => "commit";
            public override string Description => "Saves changes to the repository.";
            public override void Execute(string[] args)
            {
                // Repository has to be initialised for command to run
                if (!Directory.Exists(Paths.janusDir))
                {
                    Logger.Log("Error: Not a janus repository. Run 'janus init' first.");
                    return;
                }

                // Commit message is required
                if (args.Length < 1)
                {
                    Logger.Log("No commit message given. Use 'janus commit \"Commit message\"'");
                    return;
                }

                // If no files have been staged then there is nothing to commit
                if (!File.Exists(Paths.index) || !File.ReadLines(Paths.index).Any())
                {
                    Logger.Log("No changes to commit.");
                    return;
                }



                Dictionary<string, string> fileHashes = new Dictionary<string, string>();
                foreach (var line in File.ReadAllLines(Paths.index))
                {
                    var parts = line.Split('|');

                    string relativeFilePath = parts[0];
                    string fileHash = parts[1];

                    string currentDir = Directory.GetCurrentDirectory();
                    string fullPath = Path.Combine(currentDir, relativeFilePath);

                    if (!File.Exists(fullPath))
                    {
                        Logger.Log($"Warning: Staged file '{relativeFilePath}' '{fullPath}' no longer exists.");

                        continue;
                    }

                    string content = File.ReadAllText(relativeFilePath);
                    fileHashes[relativeFilePath] = fileHash;

                    // Write file content to objects directory
                    string objectFilePath = Path.Combine(Paths.objectDir, fileHash);
                    if (!File.Exists(objectFilePath)) // Dont rewrite existing objects
                    {
                        File.WriteAllText(objectFilePath, content);
                    }
                }


                string commitMessage = args[0];

                // Generate commit metadata
                string commitHash = CommandHelper.ComputeCommitHash(fileHashes, commitMessage);
                string parentCommit = CommandHelper.GetCurrentHead();
                string commitMetadata = CommandHelper.GenerateCommitMetadata(commitHash, fileHashes, commitMessage, parentCommit);

                // Save commit object
                string commitFilePath = Path.Combine(Paths.commitDir, commitHash);
                File.WriteAllText(commitFilePath, commitMetadata);

                // Update head to point to the new commit
                HeadHelper.SetHeadCommit(commitHash);

                // Clear the staging area
                File.WriteAllText(Paths.index, string.Empty);

                Logger.Log($"Committed as {commitHash}");

            }
        }


        /*
        public class PushCommand : BaseCommand
        {
            public override string Name => "push";
            public override string Description => "Pushes the local repository to the remote repository.";
            public override void Execute(string[] args)
            {
                try
                {
                    Logger.Log("Attempting");
                    string commitJson = PushHelper.GetCommitMetadataFiles(); // await
                    Logger.Log("Finished commitmetadata: " + commitJson);
                    // Get branch header
                    // TODO


                    // Send to backend
                    PushHelper.PostToBackendAsync(commitJson).GetAwaiter().GetResult(); // await
                }
                catch (Exception ex)
                {
                    Logger.Log("Failed: " + ex);
                }




            }

        }
        */
















        /*
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
        */
    }
}
