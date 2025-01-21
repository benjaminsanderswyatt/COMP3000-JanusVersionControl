using Janus.Helpers;
using Janus.Models;
using Janus.Plugins;
using System.Data;
using System.Text.Json;


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
                new LogCommand(logger, paths),
                /*
                new ListBranchesCommand(logger, paths),
                new CreateBranchCommand(logger, paths),
                new DeleteBranchCommand(logger, paths),
                new SwitchBranchCommand(logger, paths),
                new SwitchCommitCommand(logger, paths),
                */
                new StatusCommand(logger, paths),
                
                //new PushCommand(),
                
                
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
                    if (Directory.Exists(Paths.JanusDir))
                    {
                        Logger.Log("Repository already initialized");
                        return;
                    }

                    Directory.CreateDirectory(Paths.JanusDir);
                    File.SetAttributes(Paths.JanusDir, File.GetAttributes(Paths.JanusDir) | FileAttributes.Hidden); // Makes the janus folder hidden


                    Directory.CreateDirectory(Paths.ObjectDir); // .janus/objects folder
                    Directory.CreateDirectory(Paths.TreeDir); // .janus/trees folder
                    Directory.CreateDirectory(Paths.HeadsDir); // .janus/heads
                    Directory.CreateDirectory(Paths.PluginsDir); // .janus/plugins folder
                    Directory.CreateDirectory(Paths.CommitDir); // .janus/commits folder
                    Directory.CreateDirectory(Paths.BranchesDir); // .janus/branches folder


                    // Create index file
                    File.Create(Paths.Index).Close();

                    // Create HEAD file pointing at main branch
                    File.WriteAllText(Paths.HEAD, "ref: heads/main");



                    // Create initial commit
                    string initialCommitMessage = "Initial commit";
                    var emptyTreeHash = "";
                    string initCommitHash = HashHelper.ComputeCommitHash(emptyTreeHash, initialCommitMessage);

                    string commitMetadata = CommandHelper.GenerateCommitMetadata("main", initCommitHash, emptyTreeHash, initialCommitMessage, null, null);

                    // Save the commit object in the commit directory
                    string commitFilePath = Path.Combine(Paths.CommitDir, initCommitHash);
                    File.WriteAllText(commitFilePath, commitMetadata);

                    // Create main branch in heads/ pointing to initial commit
                    File.WriteAllText(Path.Combine(Paths.HeadsDir, "main"), initCommitHash);

                    // Create detached Head file
                    File.WriteAllText(Paths.DETACHED_HEAD, initCommitHash);




                    // Create branches file for main
                    var branch = new Branch
                    {
                        Name = "main",
                        InitialCommit = initCommitHash,
                        CreatedBy = null,
                        ParentBranch = null,
                        Created = DateTimeOffset.Now
                    };

                    string branchJson = JsonSerializer.Serialize(branch, new JsonSerializerOptions { WriteIndented = true });

                    // Create branch folder and store the branch info and index
                    string branchFolderPath = Path.Combine(Paths.BranchesDir, "main");
                    Directory.CreateDirectory(branchFolderPath);
                    File.WriteAllText(Path.Combine(branchFolderPath, "info"), branchJson);

                    File.Create(Path.Combine(branchFolderPath, "index")).Close();




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
                // Repository has to be initialised for command to run
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                // No arguments given so command should return error
                if (args.Length < 1)
                {
                    Logger.Log("No files or directory specified to add.");
                    return;
                }

                if (args.Any(arg => arg.ToLowerInvariant().Equals("all", StringComparison.OrdinalIgnoreCase)))
                {
                    Logger.Log("Warning using 'all' will override other arguments.");

                    args = new string[] { Paths.WorkingDir }; // replaces args with 1 argument of the whole working directory
                }

                // Load existing staged files
                var stagedFiles = IndexHelper.LoadIndex(Paths.Index);




                var filesToAdd = new List<string>();
                var deletedFiles = new List<string>();

                foreach (var arg in args)
                {
                    // Ensure the path is a fullpath
                    var argument = Path.Combine(Paths.WorkingDir, arg);
                    

                    if (Directory.Exists(argument)) // Directory
                    {
                        // Get all files in the given directory recursively
                        var directoryFiles = CommandHelper.GetAllFilesInDir(Paths, argument);

                        // Handle files in dir
                        foreach (var filePath in directoryFiles)
                        {
                            // File exists in dir add to filesToAdd
                            filesToAdd.Add(filePath);
                        }

                        var stagedFilesInFolder = stagedFiles.Keys
                                                        .Where(filePath => filePath.StartsWith(arg, StringComparison.OrdinalIgnoreCase)
                                                            && !directoryFiles.Contains(filePath))
                                                        .ToList();

                        // Handle deleted files in dir
                        foreach (var filepath in stagedFilesInFolder)
                        {
                            deletedFiles.Add(filepath);
                        }


                    }
                    else if (File.Exists(argument)) // File
                    {
                        // Add the file
                        filesToAdd.Add(arg);
                    }
                    else // Doesnt exist -> check index
                    {
                        var stagedFilesInFolder = stagedFiles.Keys
                                                        .Where(filePath => filePath.StartsWith(arg, StringComparison.OrdinalIgnoreCase))
                                                        .ToList();

                        if (stagedFilesInFolder.Any())
                        {
                            foreach (var filePath in stagedFilesInFolder)
                            {
                                // Add to deleted files list
                                deletedFiles.Add(filePath);
                            }
                        }
                        else
                        {
                            Logger.Log($"Error: Path '{arg}' does not exist.");
                            return;
                        }

                    }
                }


                // Check .janusignore for ingored patterns
                var ignoredPatterns = File.Exists(".janusignore")
                    ? File.ReadAllLines(".janusignore").Select(pattern => pattern.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList()
                    : new List<string>();

                filesToAdd = filesToAdd.Where(file => !AddHelper.IsFileIgnored(file, ignoredPatterns)).ToList();



                // Stage each file
                foreach (string relativeFilePath in filesToAdd)
                {
                    // Compute file hash
                    var (fileHash, content) = HashHelper.ComputeHashAndGetContent(Paths.WorkingDir, relativeFilePath);


                    // If the file isnt already staged or the file has been modified stage it
                    if (!stagedFiles.ContainsKey(relativeFilePath) || stagedFiles[relativeFilePath] != fileHash)
                    {
                        // Write file content to objects directory
                        string objectFilePath = Path.Combine(Paths.ObjectDir, fileHash);
                        if (!File.Exists(objectFilePath)) // Dont rewrite existing objects (this way they can be reused)
                        {
                            File.WriteAllText(objectFilePath, content);
                        }

                        stagedFiles[relativeFilePath] = fileHash;
                        Logger.Log($"Added '{relativeFilePath}' to the staging area.");
                    }
                    else
                    {
                        Logger.Log($"File '{relativeFilePath}' is already staged.");
                    }

                }


                // Mark deleted files "Deleted"
                foreach (string relativeFilePath in deletedFiles)
                {
                    stagedFiles[relativeFilePath] = "Deleted";
                    Logger.Log($"Marked '{relativeFilePath}' as deleted.");
                }


                // Update index
                IndexHelper.SaveIndex(Paths.Index, stagedFiles);


                Logger.Log($"{filesToAdd.Count + deletedFiles.Count} files processed.");
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
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                string parentCommit;
                try
                {
                    parentCommit = CommandHelper.GetCurrentHEAD(Paths);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error getting current HEAD: {ex.Message}");
                    return;
                }


                // Validate commit message
                if (!CommitHelper.ValidateCommitMessage(Logger, args, out string commitMessage)) return;


                string branch = CommandHelper.GetCurrentBranchName(Paths);

                var stagedFiles = IndexHelper.LoadIndex(Paths.Index);

                // Check if there are any changes to commit
                if (!StatusHelper.HasAnythingBeenStagedForCommit(Paths, parentCommit, stagedFiles))
                {
                    Logger.Log("No changes to commit.");
                    return;
                }



                var treeStructure = new Dictionary<string, object>();

                foreach (var file in stagedFiles)
                {
                    string relativeFilePath = file.Key;
                    string fileHash = file.Value;

                    if (fileHash == "Deleted")
                    {
                        // Add to tree
                        string[] pathParts = relativeFilePath.Split(Path.DirectorySeparatorChar);
                        TreeHelper.AddToTreeRecursive(treeStructure, pathParts, fileHash);

                    }
                    else
                    {
                        try
                        {
                            // Add to tree
                            string[] pathParts = relativeFilePath.Split(Path.DirectorySeparatorChar);
                            TreeHelper.AddToTreeRecursive(treeStructure, pathParts, fileHash);

                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"Error committing file '{relativeFilePath}': {ex.Message}");
                            return;
                        }
                    }
                }

                try
                {
                    // Create tree object and store in trees
                    string treeJson = JsonSerializer.Serialize(treeStructure, new JsonSerializerOptions { WriteIndented = true });
                    string rootTreeHash = HashHelper.ComputeHash(treeJson);
                    string treeFilePath = Path.Combine(Paths.TreeDir, rootTreeHash);

                    File.WriteAllText(treeFilePath, treeJson);

                    // Generate commit metadata
                    string commitHash = HashHelper.ComputeCommitHash(rootTreeHash, commitMessage);
                    string commitMetadata = CommandHelper.GenerateCommitMetadata(branch, commitHash, rootTreeHash, commitMessage, parentCommit, CommandHelper.GetUsername());

                    // Save commit object
                    string commitFilePath = Path.Combine(Paths.CommitDir, commitHash);
                    File.WriteAllText(commitFilePath, commitMetadata);

                    // Update head to point to the new commit
                    HeadHelper.SetHeadCommit(Paths, commitHash);


                    // Clean up index removing deleted files
                    var updatedIndex = stagedFiles.Where(kv => kv.Value != "Deleted")
                                          .ToDictionary(kv => kv.Key, kv => kv.Value);
                    IndexHelper.SaveIndex(Paths.Index, updatedIndex);

                    Logger.Log($"Committed as {commitHash}");

                }
                catch (Exception ex)
                {
                    Logger.Log($"Error saving commit: {ex.Message}");
                }
            }
        }










        public class LogCommand : BaseCommand
        {
            public LogCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "log";
            public override string Description => "Displays the commit history. 'janus log branch=<> author=<> since=<> until=<> limit=<> verbose=<>'";
            public override void Execute(string[] args)
            {
                // Repository has to be initialised for command to run
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                if (!Directory.Exists(Paths.CommitDir))
                {
                    Logger.Log("Error commit directory doesnt exist.");
                    return;
                }


                // Get all commit files
                var commitFiles = Directory.GetFiles(Paths.CommitDir);

                if (commitFiles.Length == 0)
                {
                    Logger.Log("Error no commits found. Repository might not have initialized correctly.");
                    return;
                }


                // Get arguments for filters

                LogFilters filters = new LogFilters
                {
                    Branch = args.FirstOrDefault(arg => arg.StartsWith("branch="))?.Split('=')[1],
                    Author = args.FirstOrDefault(arg => arg.StartsWith("author="))?.Split('=')[1],
                    Since = args.FirstOrDefault(arg => arg.StartsWith("since="))?.Split('=')[1],
                    Until = args.FirstOrDefault(arg => arg.StartsWith("until="))?.Split('=')[1],
                    Limit = args.FirstOrDefault(arg => arg.StartsWith("limit="))?.Split('=')[1]
                };


                List<CommitMetadata?> commitPathsInFolder;
                try
                {
                    commitPathsInFolder = commitFiles
                        .Select(file => JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(file)))
                        .Where(metadata => metadata != null) // Exclude invalid or null metadata
                        .Where(metadata =>

                            // Filter by branch
                            (string.IsNullOrEmpty(filters.Branch) || metadata.Branch.Equals(filters.Branch, StringComparison.OrdinalIgnoreCase)) &&
                            // Filter by author
                            (string.IsNullOrEmpty(filters.Author) ||
                                (metadata.Author != null && metadata.Author.Equals(filters.Author, StringComparison.OrdinalIgnoreCase))) &&
                            // Filter by date range
                            (string.IsNullOrEmpty(filters.Since) || metadata.Date >= DateTimeOffset.Parse(filters.Since)) &&
                            (string.IsNullOrEmpty(filters.Until) || metadata.Date <= DateTimeOffset.Parse(filters.Until))

                        )
                        .OrderBy(metadata => metadata.Date)
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.Log("Error reading commit files: " + ex.Message);
                    return;
                }

                // Apply the limit if specified
                if (int.TryParse(filters.Limit, out int limit))
                {
                    commitPathsInFolder = commitPathsInFolder.Take(limit).ToList();
                }


                // Display commit history
                if (!commitPathsInFolder.Any())
                {
                    Logger.Log("No commits match the provided filters.");
                    return;
                }


                // Display commit history
                foreach (var commit in commitPathsInFolder)
                {
                    // Color code the commit output

                    // Commit Hash
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Logger.Log($"Commit:  {commit.Commit}");

                    // Author
                    Console.ForegroundColor = ConsoleColor.Green;
                    Logger.Log($"Author:  {commit.Author}");

                    // Date
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Logger.Log($"Date:    {commit.Date}");

                    // Branch
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Logger.Log($"Branch:  {commit.Branch}");

                    // Message
                    Console.ForegroundColor = ConsoleColor.White;
                    Logger.Log($"Message: {commit.Message}");


                    // Reset color after each commit log
                    Console.ResetColor();
                    CommandHelper.DisplaySeperator(Logger);
                }



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




        public class CreateBranchCommand : BaseCommand
        {
            public CreateBranchCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "create_branch";
            public override string Description => "Creates a branch from the head commit of current branch.";
            public override void Execute(string[] args)
            {
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                if (args.Length < 1)
                {
                    Logger.Log("Please provide a branch name.");
                    return;
                }

                string branchName = args[0];
                if (!BranchHelper.IsValidBranchName(branchName))
                {
                    Logger.Log($"Invalid branch name: {branchName}");
                    return;
                }


                string branchPath = Path.Combine(Paths.HeadsDir, branchName);

                if (File.Exists(branchPath))
                {
                    Logger.Log($"Branch '{branchName}' already exists.");
                    return;
                }


                // Put the latest commit of the current branch into the new branch file
                string branchHeadCommit = CommandHelper.GetCurrentHEAD(Paths);

                File.WriteAllText(branchPath, branchHeadCommit);

                // Create branches file for main
                var branch = new Branch
                {
                    Name = branchName,
                    InitialCommit = branchHeadCommit,
                    CreatedBy = CommandHelper.GetUsername(),
                    ParentBranch = CommandHelper.GetCurrentBranchName(Paths),
                    Created = DateTimeOffset.Now
                };

                string branchJson = JsonSerializer.Serialize(branch, new JsonSerializerOptions { WriteIndented = true });


                string branchFolderPath = Path.Combine(Paths.BranchesDir, branchName);
                Directory.CreateDirectory(branchFolderPath);
                File.WriteAllText(Path.Combine(branchFolderPath, branchName), branchJson);

                File.Copy(Paths.Index, Path.Combine(branchFolderPath, "index"));


                Logger.Log($"Created new branch {branchName}");
            }
        }

        public class DeleteBranchCommand : BaseCommand
        {
            public DeleteBranchCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "delete_branch";
            public override string Description => "Deletes a branch.";
            public override void Execute(string[] args)
            {
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                if (args.Length < 1)
                {
                    Logger.Log("Please provide a branch name.");
                    return;
                }

                string branchName = args[0];

                // Check if user is in branch
                string currentBranch = CommandHelper.GetCurrentBranchName(Paths);
                if (currentBranch == branchName)
                {
                    Logger.Log($"Cannot delete branch '{branchName}' while on it.");
                    return;
                }


                string branchPath = Path.Combine(Paths.HeadsDir, branchName);

                if (!File.Exists(branchPath))
                {
                    Logger.Log($"Branch '{branchName}' doesnt exists.");
                    return;
                }


                // Prompt user to confirm deletion
                if (!CommandHelper.ConfirmAction(Logger, $"Are you sure you want to delete branch '{branchName}'?"))
                {
                    Logger.Log("Branch deletion cancelled.");
                    return;
                }


                try
                {
                    // Delete the branch commit
                    BranchHelper.DeleteBranchCommitAndFiles(Logger, Paths, branchName);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error deleting branch '{branchName}': {ex.Message}");
                    return;
                }

                


                // Delete the branch file
                File.Delete(branchPath);

                // Delete the branch file in branches directory
                Directory.Delete(Path.Combine(Paths.BranchesDir, branchName), true);



                Logger.Log($"Deleted branch {branchName}.");
            }
        }




        public class ListBranchesCommand : BaseCommand
        {
            public ListBranchesCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "list_branch";
            public override string Description => "list branch help";
            public override void Execute(string[] args)
            {
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                // Get all branches in heads directory
                string[] allBranchesPaths = Directory.GetFiles(Paths.HeadsDir);

                var currentBranch = CommandHelper.GetCurrentBranchName(Paths);


                foreach (string branchPath in allBranchesPaths)
                {
                    string branch = Path.GetFileName(branchPath);

                    if (branch == currentBranch)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;

                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                    }

                    Logger.Log(branch);

                    Console.ResetColor();
                }


            }
        }



        public class SwitchBranchCommand : BaseCommand
        {
            public SwitchBranchCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "switch_branch";
            public override string Description => "switch branch help";
            public override void Execute(string[] args)
            {
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                if (args.Length < 1)
                {
                    Logger.Log("Please provide a branch name.");
                    return;
                }

                string branchName = args[0];
                string branchPath = Path.Combine(Paths.HeadsDir, branchName);

                if (!File.Exists(branchPath))
                {
                    Logger.Log($"Branch '{branchName}' doesnt exists.");
                    return;
                }

                // TODO
                // Check if there are any uncommitted changes if so prompt user to confirm (uncommitted wont be saved)
                // if(ConfirmAction($"")) 



                try
                {
                    // Update the working directory with the files from the new branch
                    BranchHelper.UpdateWorkingDirectory(Logger, Paths, branchName);


                    // Store the index state into the previous branch
                    string previousBranch = CommandHelper.GetCurrentBranchName(Paths);
                    File.Copy(Paths.Index, Path.Combine(Paths.BranchesDir, previousBranch, "index"));

                    // Load the index for new branch
                    File.Copy(Path.Combine(Paths.BranchesDir, previousBranch, "index"), Paths.Index);


                    // Update HEAD to point to the switched to branch
                    File.WriteAllText(Paths.HEAD, $"ref: heads/{branchName}");


                    Logger.Log($"Switched to branch {branchName}.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error switching branch '{branchName}': {ex.Message}");
                    return;
                }

            }
        }




        public class SwitchCommitCommand : BaseCommand
        {
            public SwitchCommitCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "switch_commit";
            public override string Description => "switch commit help";
            public override void Execute(string[] args)
            {
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                if (args.Length < 1)
                {
                    Logger.Log("Please provide a commit hash.");
                    return;
                }

                string commitHash = args[0];
                string commitPath = Path.Combine(Paths.CommitDir, commitHash);

                if (!File.Exists(commitPath))
                {
                    Logger.Log($"Commit '{commitHash}' doesnt exists.");
                    return;
                }

                // TODO
                // Check if there are any uncommitted changes if so prompt user to confirm (uncommitted wont be saved)
                // if(ConfirmAction($"")) 


                try
                {
                    // Update the working directory with the files from the CommitHash
                    BranchHelper.UpdateWorkingDirectory(Logger, Paths, commitHash);

                    // Update HEAD to point to the detached HEAD
                    File.WriteAllText(Paths.HEAD, $"ref: {Paths.DETACHED_HEAD}");

                    Logger.Log($"Switched to commit {commitHash}.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error switching to commit '{commitHash}': {ex.Message}");
                    return;
                }

            }
        }

        */


        public class StatusCommand : BaseCommand
        {
            public StatusCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "status";
            public override string Description => "Displays the status of the repository.";
            public override void Execute(string[] args)
            {
                if (!CommandHelper.ValidateRepoExists(Logger, Paths)) { return; }

                // Get the current branch
                string currentBranch = CommandHelper.GetCurrentBranchName(Paths);
                Logger.Log($"On branch:");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Logger.Log($"    {currentBranch}");
                Console.ResetColor();
                CommandHelper.DisplaySeperator(Logger);

                // TODO Check the status of the current branch against the remote branch
                // Show branch sync status
                Logger.Log("Remote sync status will be displayed here.");
                CommandHelper.DisplaySeperator(Logger);


                var stagedFiles = IndexHelper.LoadIndex(Paths.Index);

                // Get files from working directory (excluding .janus files)
                var workingFiles = CommandHelper.GetAllFilesInDir(Paths, Paths.WorkingDir).ToList();


                // Check .janusignore for ingored patterns
                var ignoredPatterns = File.Exists(".janusignore")
                    ? File.ReadAllLines(".janusignore").Select(pattern => pattern.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList()
                    : new List<string>();

                workingFiles = workingFiles.Where(file => !AddHelper.IsFileIgnored(file, ignoredPatterns)).ToList();



                // Get the tree from the HEAD commit
                string headCommitHash = CommandHelper.GetCurrentHEAD(Paths);
                Dictionary<string, object> tree = TreeHelper.GetTreeFromCommitHash(Paths, headCommitHash);


                // Staged for commit
                var (stagedForCommitModified, stagedForCommitAdded, stagedForCommitDeleted) = StatusHelper.GetNotStagedUntracked(tree, stagedFiles);


                if (stagedForCommitModified.Any() || stagedForCommitDeleted.Any() || stagedForCommitAdded.Any())
                {
                    Logger.Log("Changes to be committed:");

                    StatusHelper.DisplayStatus(Logger, stagedForCommitAdded, ConsoleColor.Green, "(added)");
                    StatusHelper.DisplayStatus(Logger, stagedForCommitModified, ConsoleColor.Yellow, "(modified)");
                    StatusHelper.DisplayStatus(Logger, stagedForCommitDeleted, ConsoleColor.Red, "(deleted)");


                    CommandHelper.DisplaySeperator(Logger);
                }
                // else -> Nothing to commit




                // Not staged files
                // Untracked
                var (notStaged, untracked) = StatusHelper.GetNotStagedUntracked(Paths.WorkingDir, workingFiles, stagedFiles);


                if (notStaged.Any())
                {
                    Logger.Log("Changes not staged for commit:");
                    StatusHelper.DisplayStatus(Logger, notStaged, ConsoleColor.Cyan);
                    CommandHelper.DisplaySeperator(Logger);
                }

                if (untracked.Any())
                {
                    Logger.Log("Untracked files:");
                    StatusHelper.DisplayStatus(Logger, untracked, ConsoleColor.Blue);
                    CommandHelper.DisplaySeperator(Logger);
                }



                // When there is nothing to commit, stage or being untracked
                if (!(stagedForCommitModified.Any() || stagedForCommitDeleted.Any() || stagedForCommitAdded.Any() || notStaged.Any() || untracked.Any()))
                {
                    Logger.Log("All clean.");
                }

            }
        }






    }
}
