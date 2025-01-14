using Janus.Helpers;
using Janus.Plugins;
using System.Text;


namespace Janus
{
    public static class CommandHandler
    {
        public static List<ICommand> GetCommands(ILogger logger, Paths paths)
        {
            var commands = new List<ICommand>
            {
                new TestCommand(logger, paths),
                new HelpCommand(logger, paths),
                new InitCommand(logger, paths),
                new AddCommand(logger, paths),
                new CommitCommand(logger, paths),
                //new PushCommand(),
                //new CreateBranchCommand(),
                //new SwitchBranchCommand(),
                //new LogCommand()
                
                // Add new built in commands here
            };

            return commands;
        }

        public class TestCommand : BaseCommand
        {
            public TestCommand(ILogger logger, Paths paths) : base(logger, paths) { }
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
            public HelpCommand(ILogger logger, Paths paths) : base(logger, paths) { }

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
            public InitCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "init";
            public override string Description => "Initializes the janus repository.";
            public override void Execute(string[] args)
            {
                try
                {
                    // Initialise .janus folder
                    if (!Directory.Exists(Paths.JanusDir))
                    {
                        Directory.CreateDirectory(Paths.JanusDir);
                        File.SetAttributes(Paths.JanusDir, File.GetAttributes(Paths.JanusDir) | FileAttributes.Hidden); // Makes the janus folder hidden
                    }
                    else
                    {
                        Logger.Log("Repository already initialized");
                        return;
                    }

                    Directory.CreateDirectory(Paths.ObjectDir); // .janus/object folder
                    Directory.CreateDirectory(Paths.RefsDir); // .janus/refs
                    Directory.CreateDirectory(Paths.HeadsDir); // .janus/refs/heads
                    Directory.CreateDirectory(Paths.PluginsDir); // .janus/plugins folder


                    // Create index file
                    File.Create(Paths.Index).Close();

                    // Create empty main branch in refs/heads/
                    File.WriteAllText(Path.Combine(Paths.HeadsDir, "main"), string.Empty);

                    // Create HEAD file pointing at main branch
                    File.WriteAllText(Paths.Head, "ref: refs/heads/main");

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
            public AddCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "add";
            public override string Description => "Adds files to the staging area. To add all files use 'janus add all'.";
            public override void Execute(string[] args)
            {
                // No arguments given so command should return error
                if (args.Length < 1)
                {
                    Logger.Log("No files or directory specified to add.");
                    return;
                }

                // Repository has to be initialised for command to run
                if (!Directory.Exists(Paths.JanusDir))
                {
                    Logger.Log("Not a janus repository. Use 'init' command to initialise repository.");
                    return;
                }

                var filesToAdd = new List<string>();

                foreach (var arg in args)
                {
                    if (Directory.Exists(arg))
                    {
                        // Add all files in the directory recursively
                        var directoryFiles = Directory.EnumerateFiles(arg, "*", SearchOption.AllDirectories)
                                                      .Select(filePath => Path.GetRelativePath(".", filePath))
                                                      .Where(path => !path.StartsWith(".janus"));
                        filesToAdd.AddRange(directoryFiles);
                    }
                    else if (File.Exists(arg))
                    {
                        // Add the file
                        filesToAdd.Add(arg);
                    }
                    else
                    {
                        Logger.Log($"Path '{arg}' does not exist.");
                    }
                }



                // Check .janusignore for ingored patterns
                var ignoredPatterns = File.Exists(".janusignore")
                    ? File.ReadAllLines(".janusignore").Select(pattern => pattern.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList()
                    : new List<string>();

                filesToAdd = filesToAdd.Where(file => !AddHelper.IsFileIgnored(file, ignoredPatterns)).ToList();


                // Load existing staged files
                var stagedFiles = AddHelper.LoadIndex(Paths.Index);


                // Remove deleted files from the staging area
                var filesToRemove = stagedFiles.Keys.Where(filePath => !File.Exists(filePath)).ToList();
                foreach (var filePath in filesToRemove)
                {
                    stagedFiles.Remove(filePath);
                }

                Logger.Log($"{filesToRemove.Count} files removed from staging area.");






                // Stage each file
                foreach (string relativeFilePath in filesToAdd)
                {
                    // File doesnt exist so it returns error and continues staging other files
                    if (!File.Exists(relativeFilePath))
                    {
                        Logger.Log($"File at '{relativeFilePath}' not found.");
                        continue;
                    }

                    try
                    {
                        // Compute file hash
                        string fileHash = AddHelper.ComputeHash_GivenFilepath(relativeFilePath);

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
                    catch (Exception ex)
                    {
                        Logger.Log($"Failed adding '{relativeFilePath}': {ex.Message}");
                    }

                }


                // Update index
                AddHelper.SaveIndex(Paths.Index, stagedFiles);

                Logger.Log($"{filesToAdd.Count} files processed.");
                Logger.Log($"{stagedFiles.Count} files in staging area.");
            }
        }








        public class CommitCommand : BaseCommand
        {
            public CommitCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "commit";
            public override string Description => "Saves changes to the repository.";
            public override void Execute(string[] args)
            {
                // Repository has to be initialised for command to run
                if (!Directory.Exists(Paths.JanusDir))
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
                if (!File.Exists(Paths.Index) || !File.ReadLines(Paths.Index).Any())
                {
                    Logger.Log("No changes to commit.");
                    return;
                }



                Dictionary<string, string> fileHashes = new Dictionary<string, string>();
                foreach (var line in File.ReadAllLines(Paths.Index))
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
                    string objectFilePath = Path.Combine(Paths.ObjectDir, fileHash);
                    if (!File.Exists(objectFilePath)) // Dont rewrite existing objects
                    {
                        File.WriteAllText(objectFilePath, content);
                    }
                }


                string commitMessage = args[0];

                // Generate commit metadata
                string commitHash = CommandHelper.ComputeCommitHash(fileHashes, commitMessage);
                string parentCommit = CommandHelper.GetCurrentHead(Paths);
                string commitMetadata = CommandHelper.GenerateCommitMetadata(commitHash, fileHashes, commitMessage, parentCommit);

                // Save commit object
                string commitFilePath = Path.Combine(Paths.CommitDir, commitHash);
                File.WriteAllText(commitFilePath, commitMetadata);

                // Update head to point to the new commit
                HeadHelper.SetHeadCommit(Paths, commitHash);

                // Clear the staging area
                File.WriteAllText(Paths.Index, string.Empty);

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
