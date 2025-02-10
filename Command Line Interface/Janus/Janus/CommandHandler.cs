using Janus.API;
using Janus.CommandHelpers;
using Janus.Helpers;
using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using System.Data;
using System.Net.Http.Headers;
using System.Text;
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

                new LoginCommand(logger, paths),

                new InitCommand(logger, paths),
                new AddCommand(logger, paths),
                new CommitCommand(logger, paths),
                new LogCommand(logger, paths),
                new ListBranchesCommand(logger, paths),
                new CreateBranchCommand(logger, paths),
                //new DeleteBranchCommand(logger, paths),
                new SwitchBranchCommand(logger, paths),
                //new SwitchCommitCommand(logger, paths),
                
                new StatusCommand(logger, paths),

                new DiffCommand(logger, paths),
                
                //new PushCommand(),
                
                
                // Add new built in commands here
            };

            return commands;
        }

        public class DiffCommand : BaseCommand
        {
            public DiffCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "diff";
            public override string Description => "Displays the difference between two files.";
            public override async Task Execute(string[] args)
            {
                string file1 = args[0];
                string file2 = args[1];

                string before = File.ReadAllText(file1);
                string after = File.ReadAllText(file2);

                Diff.TestDiff(before, after);


                // This command will
                // Read the metadata of the commit
                // get the tree and decifer the commits 
                // work out the diff delta between the commit files
                // display the diff

                // file name - path
                // diff




            }
        }


        public class TestCommand : BaseCommand
        {
            public TestCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "test";
            public override string Description => "Send a test request to backend";
            public override async Task Execute(string[] args)
            {
                await MiscHelper.ExecuteAsync();
                Logger.Log("Test End");
            }
        }


        public class HelpCommand : BaseCommand
        {
            public HelpCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "help";
            public override string Description => "Displays a list of available commands.";
            public override async Task Execute(string[] args)
            {
                Logger.Log("Usage: janus <command>");
                Logger.Log("Commands:");
                foreach (var command in Program.CommandList)
                {
                    Logger.Log($"{command.Name.PadRight(20)} : {command.Description}");
                }
            }
        }

        public class LoginCommand : BaseCommand
        {
            public LoginCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "login";
            public override string Description => "Authenticate user";
            public override async Task Execute(string[] args)
            {
                try
                {
                    // Prompt for credentials

                    Console.WriteLine("Email: ");
                    var email = Console.ReadLine();

                    if (string.IsNullOrEmpty(email))
                    {
                        Logger.Log("Email is required");
                        return;
                    }

                    Console.WriteLine("Personal access token (PAT): ");
                    var pat = Console.ReadLine();

                    if (string.IsNullOrEmpty(pat))
                    {
                        Logger.Log("The personal access token is required");
                        return;
                    }

                    Console.WriteLine("Before");

                    // Auth on backend
                    var (success, username, token) = await PatAuth.SendAuthenticateAsync(Logger, pat, email);
                    
                    
                    Console.WriteLine("After");

                    if (success)
                    {
                        var credentials = new UserCredentials
                        {
                            Username = username,
                            Email = email,
                            Token = token
                        };

                        // Save credentials
                        var credManager = new CredentialManager();
                        credManager.SaveCredentials(credentials);

                        Logger.Log($"Successfully logged in as {credentials.Username} ({credentials.Email})");
                    }
                    else
                    {
                        Logger.Log("Authentication failed");
                    }

                } 
                catch (HttpRequestException ex)
                {
                    Logger.Log($"Network error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Login error: {ex.Message}");
                }
            }
        }
        
        
        








        public class InitCommand : BaseCommand
        {
            public InitCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "init";
            public override string Description => "Initializes the janus repository.";
            public override async Task Execute(string[] args)
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
                    var (initCommitHash, commitMetadata) = MiscHelper.CreateInitData();

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
            public override async Task Execute(string[] args)
            {
                // Repository has to be initialised for command to run
                if (!MiscHelper.ValidateRepoExists(Logger, Paths)) { return; }

                // No arguments given so command should return error
                if (args.Length < 1)
                {
                    Logger.Log("No files or directory specified to add.");
                    return;
                }

                if (args.Any(arg => arg.ToLowerInvariant().Equals("all", StringComparison.OrdinalIgnoreCase)))
                {
                    args = new string[] { "" }; // replaces args with 1 argument of the whole working directory
                }

                // Load existing staged files
                var stagedFiles = IndexHelper.LoadIndex(Paths.Index);

                // Load ignore patterns
                var (includePatterns, excludePatterns) = IgnoreHelper.LoadIgnorePatterns(Paths.WorkingDir);


                var filesToAdd = new List<string>();
                var deletedFiles = new List<string>();

                foreach (var arg in args)
                {
                    string relPath = arg.Replace('/', Path.DirectorySeparatorChar);

                    if (IgnoreHelper.ShouldIgnore(relPath, includePatterns, excludePatterns))
                    {
                        Logger.Log($"Ignoring '{relPath}' due to '.janusignore' file");
                        continue;
                    }

                    // Ensure the path is a fullpath
                    var fullPath = Path.Combine(Paths.WorkingDir, relPath);


                    if (Directory.Exists(fullPath)) // Directory
                    {
                        // Get all files in the given directory recursively
                        var directoryFiles = GetFilesHelper.GetAllFilesInDir(Paths, fullPath);

                        // Handle files in dir
                        foreach (var filePath in directoryFiles)
                        {
                            // File exists in dir add to filesToAdd
                            filesToAdd.Add(filePath);
                        }

                        var stagedFilesInFolder = stagedFiles.Keys
                                                        .Where(filePath => filePath.StartsWith(relPath, StringComparison.Ordinal)
                                                            && !directoryFiles.Contains(filePath))
                                                        .ToList();

                        // Handle deleted files in dir
                        foreach (var filepath in stagedFilesInFolder)
                        {
                            deletedFiles.Add(filepath);
                        }


                    }
                    else if (File.Exists(fullPath)) // File
                    {
                        // Add the file
                        filesToAdd.Add(relPath);
                    }
                    else // Doesnt exist -> check index
                    {
                        var stagedFilesInFolder = stagedFiles.Keys
                                                        .Where(filePath => filePath.StartsWith(relPath, StringComparison.Ordinal))
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
                            File.WriteAllBytes(objectFilePath, content);
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
            public override async Task Execute(string[] args)
            {
                // Repository has to be initialised for command to run
                if (!MiscHelper.ValidateRepoExists(Logger, Paths)) { return; }

                string parentCommit;
                try
                {
                    parentCommit = MiscHelper.GetCurrentHeadCommitHash(Paths);
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error getting current HEAD: {ex.Message}");
                    return;
                }


                // Validate commit message
                if (!CommitHelper.ValidateCommitMessage(Logger, args, out string commitMessage)) return;



                try
                {
                    // Get staged files
                    var stagedFiles = IndexHelper.LoadIndex(Paths.Index);
                    var stagedTreeBuilder = new TreeBuilder(Paths);

                    // Clean up index removing deleted files
                    var updatedIndex = stagedFiles.Where(kv => kv.Value != "Deleted")
                                          .ToDictionary(kv => kv.Key, kv => kv.Value);

                    var stagedTree = stagedTreeBuilder.BuildTreeFromDiction(updatedIndex);


                    // Check if there are any changes to commit
                    if (!StatusHelper.HasAnythingBeenStagedForCommit(Logger, Paths, stagedTree))
                    {
                        Logger.Log("No changes to commit.");
                        return;
                    }


                    var rootTreeHash = stagedTreeBuilder.SaveTree(); // Save index tree


                    string branch = MiscHelper.GetCurrentBranchName(Paths);

                    // Generate commit metadata rootTreeHash commitMessage
                    var username = MiscHelper.GetUsername();
                    var email = MiscHelper.GetEmail();

                    string commitHash = HashHelper.ComputeCommitHash(parentCommit, branch, username, email, DateTimeOffset.Now, commitMessage, rootTreeHash);
                    string commitMetadata = MiscHelper.GenerateCommitMetadata(branch, commitHash, rootTreeHash, commitMessage, parentCommit, username, email);


                    // Save commit object
                    string commitFilePath = Path.Combine(Paths.CommitDir, commitHash);
                    File.WriteAllText(commitFilePath, commitMetadata);

                    // Update head to point to the new commit
                    HeadHelper.SetHeadCommit(Paths, commitHash);



                    // Save cleaned up index
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
            public override string Description => "Displays the commit history. 'janus log branch=<> author=<> since=<> until=<> limit=<>'";
            public override async Task Execute(string[] args)
            {
                // Repository has to be initialised for command to run
                if (!MiscHelper.ValidateRepoExists(Logger, Paths)) { return; }

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
                        .Select(commit => JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(commit)))
                        .Where(metadata => metadata != null) // Exclude invalid or null metadata
                        .Where(metadata =>

                            // Filter by branch
                            (string.IsNullOrEmpty(filters.Branch) || metadata.Branch.Equals(filters.Branch, StringComparison.OrdinalIgnoreCase)) &&
                            // Filter by author
                            (string.IsNullOrEmpty(filters.Author) || metadata.AuthorName.Equals(filters.Author, StringComparison.OrdinalIgnoreCase)) &&
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

                    // Author Name
                    Console.ForegroundColor = ConsoleColor.Green;
                    Logger.Log($"Author name:  {commit.AuthorName}");

                    // Author Email
                    Console.ForegroundColor = ConsoleColor.Green;
                    Logger.Log($"Author email:  {commit.AuthorEmail}");

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
                    MiscHelper.DisplaySeperator(Logger);
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









        public class CreateBranchCommand : BaseCommand
        {
            public CreateBranchCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "create_branch";
            public override string Description => "Creates a branch from the head commit of current branch.";
            public override async Task Execute(string[] args)
            {
                if (!MiscHelper.ValidateRepoExists(Logger, Paths)) { return; }

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
                string branchHeadCommit = MiscHelper.GetCurrentHeadCommitHash(Paths);

                File.WriteAllText(branchPath, branchHeadCommit);

                // Create branches file for main
                var branch = new Branch
                {
                    Name = branchName,
                    InitialCommit = branchHeadCommit,
                    CreatedBy = MiscHelper.GetUsername(),
                    ParentBranch = MiscHelper.GetCurrentBranchName(Paths),
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



        /*
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


                // Check if the repo is clean as the switch will override uncommitted changes
                bool force = args.Contains("--force") ? true : false;
                if (!force) // Force skips the check
                {
                    // Promt user to confirm branch switch
                    if (!CommandHelper.ConfirmAction(Logger, $"Are you sure you want to delete branch '{branchName}'?", force))
                    {
                        Logger.Log("Branch deletion cancelled.");
                        return;
                    }
                    
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
        */



        public class ListBranchesCommand : BaseCommand
        {
            public ListBranchesCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "list_branch";
            public override string Description => "list branch help";
            public override async Task Execute(string[] args)
            {
                if (!MiscHelper.ValidateRepoExists(Logger, Paths)) { return; }

                // Get all branches in heads directory
                string[] allBranchesPaths = Directory.GetFiles(Paths.HeadsDir);

                var currentBranch = MiscHelper.GetCurrentBranchName(Paths);

                Logger.Log("Branches:");
                foreach (string branchPath in allBranchesPaths)
                {
                    string branch = Path.GetFileName(branchPath);

                    if (branch == currentBranch)
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Logger.Log($"-> {branch}");
                    }
                    else
                    {
                        Logger.Log($"   {branch}");
                    }

                    Console.ResetColor();
                }


            }
        }



        public class SwitchBranchCommand : BaseCommand
        {
            public SwitchBranchCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "switch_branch";
            public override string Description => "switch branch help";
            public override async Task Execute(string[] args)
            {
                if (!MiscHelper.ValidateRepoExists(Logger, Paths)) { return; }

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

                string currentBranch = MiscHelper.GetCurrentBranchName(Paths);
                if (currentBranch == branchName)
                {
                    Logger.Log($"Already on branch '{branchName}'.");
                    return;
                }


                // Check if the repo is clean as the switch will override uncommitted changes
                bool force = args.Contains("--force") ? true : false;
                if (!force) // Force skips the check
                {

                    if (StatusHelper.AreThereUncommittedChanges(Logger, Paths))
                    {
                        // Promt user to confirm branch switch
                        if (!MiscHelper.ConfirmAction(Logger, "Are you sure you want to switch branches? Uncommitted changes will be lost.", force))
                        {
                            Logger.Log("Branch switch cancelled.");
                            return;
                        }
                    }

                }



                try
                {
                    // Switch Branch
                    SwitchBranchHelper.SwitchBranch(Logger, Paths, currentBranch, branchName);

                }
                catch (Exception ex)
                {
                    Logger.Log($"Error switching branch '{branchName}': {ex.Message}");
                    return;
                }

            }
        }

        /*


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
            public override async Task Execute(string[] args)
            {
                if (!MiscHelper.ValidateRepoExists(Logger, Paths)) { return; }

                // Get the current branch
                string currentBranch = MiscHelper.GetCurrentBranchName(Paths);
                Logger.Log($"On branch:");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Logger.Log($"    {currentBranch}");
                Console.ResetColor();
                MiscHelper.DisplaySeperator(Logger);

                // TODO Check the status of the current branch against the remote branch
                // Show branch sync status
                Logger.Log("Remote sync status will be displayed here.");
                MiscHelper.DisplaySeperator(Logger);




                // Get Added, Modified & Deleted files
                var addedModifiedDeleted = StatusHelper.GetAddedModifiedDeleted(Logger, Paths);


                bool anyAMD = addedModifiedDeleted.AddedOrUntracked.Any() || addedModifiedDeleted.ModifiedOrNotStaged.Any() || addedModifiedDeleted.Deleted.Any();

                if (anyAMD)
                {
                    Logger.Log("Changes to be committed:");

                    StatusHelper.DisplayStatus(Logger, addedModifiedDeleted.AddedOrUntracked, ConsoleColor.Green, "(added)");
                    StatusHelper.DisplayStatus(Logger, addedModifiedDeleted.ModifiedOrNotStaged, ConsoleColor.Yellow, "(modified)");
                    StatusHelper.DisplayStatus(Logger, addedModifiedDeleted.Deleted, ConsoleColor.Red, "(deleted)");

                    MiscHelper.DisplaySeperator(Logger);
                }
                // else -> Nothing to commit





                // Get Not Staged & Untracked files
                var notStagedUntracked = StatusHelper.GetNotStagedUntracked(Paths);

                bool anyNS = notStagedUntracked.ModifiedOrNotStaged.Any();
                bool anyU = notStagedUntracked.AddedOrUntracked.Any();

                if (anyNS)
                {
                    Logger.Log("Changes not staged for commit:");
                    StatusHelper.DisplayStatus(Logger, notStagedUntracked.ModifiedOrNotStaged, ConsoleColor.Cyan);
                    MiscHelper.DisplaySeperator(Logger);
                }

                if (anyU)
                {
                    Logger.Log("Untracked files:");
                    StatusHelper.DisplayStatus(Logger, notStagedUntracked.AddedOrUntracked, ConsoleColor.Blue);
                    MiscHelper.DisplaySeperator(Logger);
                }




                // When there is nothing to commit, stage or being untracked
                if (!(anyAMD || anyNS || anyU))
                {
                    Logger.Log("All clean.");
                }


            }
        }


    }
}
