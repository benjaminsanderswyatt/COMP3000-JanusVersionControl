using Janus.Helpers;
using Janus.Plugins;
using System.Text;


namespace Janus
{
    public static class CommandHandler
    {
        public static List<ICommand> GetCommands()
        {
            return new List<ICommand>
            {
                new TestCommand(),
                new HelpCommand(),
                new LoginCommand(),
                new LogOutCommand(),
                new InitCommand(),
                new AddCommand(),
                new CommitCommand(),
                new PushCommand(),
                //new CreateBranchCommand(),
                //new SwitchBranchCommand(),
                //new LogCommand()
                
                // Add new built in commands here
            };
        }

        public class TestCommand : ICommand
        {
            public string Name => "test";
            public string Description => "Send a test request to backend";
            public void Execute(string[] args)
            {
                //await CommandHelper.ExecuteAsync();
                Console.WriteLine("Test End");
               
            }
        }


        public class HelpCommand : ICommand
        {
            public string Name => "help";
            public string Description => "Displays a list of available commands.";
            public void Execute(string[] args)
            {
                Console.WriteLine("Usage: janus [command]");
                Console.WriteLine("Commands:");
                foreach (var command in Program.CommandList)
                {
                    Console.WriteLine($"{command.Name.PadRight(20)} : {command.Description}");
                }
            }
        }


        public class LoginCommand : ICommand
        {
            public string Name => "login";
            public string Description => "Gets a token for login.";
            public void Execute(string[] args)
            {
                Console.Write("Enter your Personal Access Token (PAT): ");
                var token = CommandHelper.ReadSecretInput();

                // Save the token
                File.WriteAllText(Paths.TokenDir, token);
                Console.WriteLine("Token saved successfully.");
            }
        }

        public class LogOutCommand : ICommand
        {
            public string Name => "logout";
            public string Description => "Removes stored token.";
            public void Execute(string[] args)
            {
                if (File.Exists(Paths.TokenDir))
                {
                    File.Delete(Paths.TokenDir);
                    Console.WriteLine("Logged out successfully.");
                }
                else
                {
                    Console.WriteLine("No token found.");
                }
            }
        }







        public class InitCommand : ICommand
        {
            public string Name => "init";
            public string Description => "Initializes the janus repository.";
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

                // .janus/commit folder
                if (!Directory.Exists(Paths.commitDir))
                    Directory.CreateDirectory(Paths.commitDir);

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
            public string Description => "Adds files to the staging area. To add all files use 'janus add all'.";
            public void Execute(string[] args)
            {
                // No arguments given so command should return error
                if (args.Length < 1)
                {
                    Console.WriteLine("No files specified.");
                    return;
                }

                // Repository has to be initialised for command to run
                if (!Directory.Exists(Paths.janusDir))
                {
                    Console.WriteLine("Not a janus repository. Run 'janus init' first.");
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
                        Console.WriteLine($"File '{relativeFilePath}' not found.");
                        continue;
                    }

                    // Compute file hash
                    string fileHash = CommandHelper.ComputeHash(File.ReadAllText(relativeFilePath));

                    // Normalize paths for comparison
                    if (!stagedFiles.ContainsKey(relativeFilePath) || stagedFiles[relativeFilePath] != fileHash)
                    {
                        stagedFiles[relativeFilePath] = fileHash;
                        Console.WriteLine($"Added '{relativeFilePath}' to the staging area.");
                    }
                    else
                    {
                        Console.WriteLine($"File '{relativeFilePath}' is already staged.");
                    }


                }

                // Update index
                File.WriteAllLines(Paths.index, stagedFiles.Select(kv => $"{kv.Key}|{kv.Value}"));

            }
        }








        public class CommitCommand : ICommand
        {
            public string Name => "commit";
            public string Description => "Saves changes to the repository.";
            public void Execute(string[] args)
            {
                // Repository has to be initialised for command to run
                if (!Directory.Exists(Paths.janusDir))
                {
                    Console.WriteLine("Error: Not a janus repository. Run 'janus init' first.");
                    return;
                }

                // Commit message is required
                if (args.Length < 1)
                {
                    Console.WriteLine("No commit message given. Use 'janus commit \"Commit message\"'");
                    return;
                }

                // If no files have been staged then there is nothing to commit
                if (!File.Exists(Paths.index) || !File.ReadLines(Paths.index).Any())
                {
                    Console.WriteLine("No changes to commit.");
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
                        Console.WriteLine($"Warning: Staged file '{relativeFilePath}' '{fullPath}' no longer exists.");

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

                Console.WriteLine($"Committed as {commitHash}");

            }
        }



        public class PushCommand : ICommand
        {
            public string Name => "push";
            public string Description => "Pushes the local repository to the remote repository.";
            public void Execute(string[] args)
            {
                try
                {
                    Console.WriteLine("Attempting");
                    string commitJson = PushHelper.GetCommitMetadataFiles(); // await
                    Console.WriteLine("Finished commitmetadata: " + commitJson);
                    // Get branch header
                    // TODO


                    // Send to backend
                    PushHelper.PostToBackendAsync(commitJson).GetAwaiter().GetResult(); // await
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed: " + ex);
                }




            }

        }

















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
