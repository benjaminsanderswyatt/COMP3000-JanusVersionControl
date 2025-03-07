using Janus.API;
using Janus.DataTransferObjects;
using Janus.Helpers;
using Janus.Helpers.CommandHelpers;
using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Data;
using System.Net;
using System.Text.Json;
using static Janus.Helpers.CommandHelpers.RemoteHelper;
using static Janus.Helpers.FileMetadataHelper;

namespace Janus
{
    public static class CommandHandler
    {
        public static List<ICommand> GetCommands(ILogger logger, Paths paths)
        {
            var commands = new List<ICommand>
            {
                new HelpCommand(logger, paths),

                new LoginCommand(logger, paths),
                new RemoteCommand(logger, paths),
                new CloneCommand(logger, paths),
                new FetchCommand(logger, paths),
                new PullCommand(logger, paths),
                new PushCommand(logger, paths),

                new InitCommand(logger, paths),
                new AddCommand(logger, paths),
                new CommitCommand(logger, paths),
                new LogCommand(logger, paths),
                new ListBranchesCommand(logger, paths),
                new CreateBranchCommand(logger, paths),
                //new DeleteBranchCommand(logger, paths),
                new SwitchBranchCommand(logger, paths),
                //new SwitchCommitCommand(logger, paths),
                //new MergeCommand(logger, paths),
                
                new StatusCommand(logger, paths),

                new DiffCommand(logger, paths),
                
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
            public override string Description => "Save your user credentials";
            public override async Task Execute(string[] args)
            {
                // Prompt for credentials

                Logger.Log("Username: ");
                var username = Console.ReadLine();
                if (string.IsNullOrEmpty(username))
                {
                    Logger.Log("Username is required");
                    return;
                }

                Logger.Log("Email: ");
                var email = Console.ReadLine().ToLower();

                if (string.IsNullOrEmpty(email))
                {
                    Logger.Log("Email is required");
                    return;
                }

                Logger.Log("Personal access token (Not required): ");
                var pat = Console.ReadLine();

                var credentials = new UserCredentials
                {
                    Username = username,
                    Email = email,
                    Token = pat
                };


                // Save credentials
                var credManager = new CredentialManager();
                credManager.SaveCredentials(credentials);

                Logger.Log($"Saved login credantials as '{username}' ({email})");
            }
        }

        public class RemoteCommand : BaseCommand
        {
            public RemoteCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "remote";
            public override string Description => "Manage your remote repositories";
            public override async Task Execute(string[] args)
            {
                if (args.Length == 0)
                {
                    Logger.Log("Provide a command");
                    return;
                }

                switch (args[0].ToLower())
                {
                    case "add":
                        await AddRemote(Logger, Paths.Remote, args);

                        break;
                    case "remove":
                        await RemoveRemote(Logger, Paths.Remote, args);

                        break;
                    case "list":
                        ListRemotes(Logger, Paths.Remote);
                        break;
                    default:
                        Logger.Log("Usage:");
                        Logger.Log("    janus remotes add <name> <link>");
                        Logger.Log("    janus remotes remote <name>");
                        Logger.Log("    janus remotes list");

                        break;
                }

            }
        }






        public class CloneCommand : BaseCommand
        {
            public CloneCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "clone";
            public override string Description => "Clone a repository from the remote repository";
            public override async Task Execute(string[] args)
            {
                if (args.Length == 0)
                {
                    Logger.Log("Repository is required");
                    return;
                }

                // Load the store user credentials
                var credManager = new CredentialManager();
                var credentials = credManager.LoadCredentials();
                if (credentials == null)
                {
                    Logger.Log("Please login first. Usage: janus login");
                    return;
                }


                string endpoint = args[0]; // janus/{owner}/{repoName}
                string[] ownerRepoData = PathHelper.PathSplitter(endpoint);
                string owner = ownerRepoData[1];
                string repoName = ownerRepoData[2];

                // Set the branch which the working dir shows
                string chosenBranch = "";
                if (args.Length > 1)
                {
                    chosenBranch = args[1];
                }

                string repoPath = Path.Combine(Directory.GetCurrentDirectory(), repoName);

                if (Directory.Exists(repoPath))
                {
                    Logger.Log($"Failed clone: Directory named '{repoName}' already exists");
                    return;
                }

                
                try
                {
                    var (success, data) = await ApiHelper.SendGetAsync(endpoint, credentials.Token);

                    if (!success)
                    {
                        Logger.Log("Error cloning repository: " + data);
                        return;
                    }



                    var cloneData = JsonSerializer.Deserialize<CloneDto>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (cloneData == null)
                    {
                        Logger.Log("Error parsing repository data.");
                        return;
                    }


                    Logger.Log($"Initialising local repository...");

                    // Create folder for repo
                    Directory.CreateDirectory(repoPath);



                    // Init repo
                    Paths clonePaths = new Paths(repoPath);
                    InitHelper.InitRepo(clonePaths, false);

                    // TODO Handle setting up branches latest commit


                    Logger.Log($"Setting repository configs...");
                    // Create repo config file
                    MiscHelper.CreateRepoConfig(clonePaths.RepoConfig, cloneData.IsPrivate, cloneData.RepoDescription);

                    // Check the chosen branch exists
                    if (!cloneData.Branches.Any(b => b.BranchName == chosenBranch))
                    {
                        // Default main and inform user
                        chosenBranch = "main";
                        Logger.Log($"Your chosen branch doesnt exist. Defaulting to main...");
                    }

                    // Store the chosen branch tree to be built
                    var chosenTree = new TreeBuilder(clonePaths);

                    // Get file hashes from commits
                    var fileHashes = new HashSet<string>();

                    foreach (var branch in cloneData.Branches)
                    {
                        Logger.Log($"Building '{branch.BranchName}' branch...");
                        string latestCommitHash = branch.LatestCommitHash;

                        foreach (var commit in branch.Commits)
                        {

                            // Save commit
                            CommitHelper.SaveCommit(clonePaths,
                                commit.CommitHash,
                                commit.ParentCommitHash,
                                branch.BranchName,
                                commit.AuthorName,
                                commit.AuthorEmail,
                                commit.CommittedAt,
                                commit.Message,
                                commit.TreeHash
                            );



                            // Save tree
                            if (commit.Tree != null && !string.IsNullOrEmpty(commit.TreeHash))
                            {
                                var treeBuilder = new TreeBuilder(clonePaths);

                                treeBuilder.LoadTree(commit.Tree);

                                string treeHash = treeBuilder.SaveTree();

                                if (treeHash != commit.TreeHash)
                                {
                                    Logger.Log($"Error tree hashes not equal. TreeHash: {treeHash}, DtoHash: {commit.TreeHash}");
                                    return;
                                }


                                if (branch.BranchName == chosenBranch)
                                {
                                    // Save tree to be rebuilt
                                    chosenTree = treeBuilder;
                                }


                                // Get file hashes from tree
                                treeBuilder.GetFileHashes(fileHashes);

                            }
                        }
                    }




                    // Download the file objects
                    Logger.Log($"Downloading file data...");

                    if (fileHashes.Any())
                    {
                        bool downloadSuccess = await ApiHelper.DownloadBatchFilesAsync(
                            owner,
                            repoName,
                            fileHashes.ToList(),
                            clonePaths.ObjectDir,
                            credentials.Token
                        );


                        if (!downloadSuccess)
                        {
                            Logger.Log("Error downloading file objects");
                            return;
                        }
                    }


                    // Recreate chosen branch branch
                    Logger.Log($"Setting up '{chosenBranch}' branch...");


                    // Build the index
                    var indexDict = chosenTree.BuildIndexDictionary();


                    // TODO
                    /*
                     */
                    // Ensure main branch dir exists
                    string chosenBranchDir = Path.Combine(clonePaths.BranchesDir, chosenBranch);
                    Directory.CreateDirectory(chosenBranchDir);
                    /*
                     */

                    // Save the index into the main branch
                    // TODO create and save the index for all branches
                    string chosenBranchIndexPath = Path.Combine (chosenBranchDir, "index");
                    IndexHelper.SaveIndex(clonePaths.Index, indexDict);
                    IndexHelper.SaveIndex(chosenBranchIndexPath, indexDict);


                    // Set HEAD to main branch
                    BranchHelper.SetCurrentHEAD(clonePaths, chosenBranch);



                    // Recreate working dir
                    Logger.Log("Recreating working directory files...");

                    var comparisonResults = Tree.CompareTrees(null, chosenTree.root);
                    foreach (var filePath in comparisonResults.AddedOrUntracked)
                    {
                        string hash = Tree.GetHashFromTree(chosenTree.root, filePath);

                        string objectFilePath = Path.Combine(clonePaths.ObjectDir, hash);
                        string destPath = Path.Combine(repoPath, filePath);

                        try
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                            File.Copy(objectFilePath, destPath, true);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"Error copying {filePath}: {ex.Message}");
                        }

                    }




                    Logger.Log($"Repository '{repoName}' successfully cloned to '{repoPath}'");

                }
                catch (Exception ex)
                {
                    Logger.Log($"An error occurred during cloning: {ex.Message}");
                }


            }

        }









        public class PushCommand : BaseCommand
        {
            public PushCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "push";
            public override string Description => "Push the local repository changes to the backend";
            public override async Task Execute(string[] args)
            {
                // Load the stored user credentials
                var credManager = new CredentialManager();
                var credentials = credManager.LoadCredentials();
                if (credentials == null)
                {
                    Logger.Log("Please login first. janus login");
                    return;
                }

                // Check the user is in a valid dir (.janus exists)
                if (Directory.Exists(Paths.JanusDir))
                {
                    Logger.Log("Local repository not found");
                    return;
                }


                // Get the repo name from folder
                // This is the name of the folder Paths.WorkingDir is in
                string repoName = new DirectoryInfo(Paths.WorkingDir).Name;
                if (BranchHelper.IsValidRepoOrBranchName(repoName))
                {
                    Logger.Log($"Invalid repository name: {repoName}");
                    return;
                }


                // Get the current branch
                string branchName = MiscHelper.GetCurrentBranchName(Paths);




                // Get the local latest commit hash

                // Get the remote latest commit hash



                // Find the differance (follow the commit history)
                // If no remote branch exists -> create remote branch (then push all commits)
                // If remote is ahead -> cancel (ask user to pull)
                // If conflict -> merge is needed

                // Once i know how far ahead local is 
                // Get the new commits and push that data




                var pushData = ""; // Temp

                // Send push
                var (success, response) = await ApiHelper.SendPostAsync($"Push/{repoName}/{branchName}", pushData, credentials.Token);

                if (success)
                {
                    Logger.Log("Push successful");
                }
                else
                {
                    Logger.Log($"Push failed: {response}");
                }

            }
        }

        public class FetchCommand : BaseCommand
        {
            public FetchCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "fetch";
            public override string Description => "fetch the latest commits from the remote repo";
            public override async Task Execute(string[] args)
            {
                // Load credentials
                var credManager = new CredentialManager();
                var credentials = credManager.LoadCredentials();
                if (credentials == null)
                {
                    Logger.Log("Please login first. janus login");
                    return;
                }

                // Check the user is in a valid dir (.janus exists)
                if (Directory.Exists(Paths.JanusDir))
                {
                    Logger.Log("Local repository not found");
                    return;
                }

                // Get the repo name from folder
                string repoName = new DirectoryInfo(Paths.WorkingDir).Name;
                if (BranchHelper.IsValidRepoOrBranchName(repoName))
                {
                    Logger.Log($"Invalid repository name: {repoName}");
                    return;
                }

                // Get the current branch
                string branchName = MiscHelper.GetCurrentBranchName(Paths);
                if (string.IsNullOrEmpty(branchName))
                {
                    Logger.Log("Failed to determine the current branch");
                    return;
                }

                Logger.Log($"Fetching latest commits...");



                // Fetch remote commits
                string owner = "temp";
                var (success, remoteCommitsJson) = await ApiHelper.SendGetAsync($"{owner}/{repoName}/{branchName}/commits", credentials.Token);
                if (!success)
                {
                    Logger.Log("Failed to fetch remote commit history.");
                    return;
                }

                List<CommitMetadata> remoteCommits = JsonSerializer.Deserialize<List<CommitMetadata>>(remoteCommitsJson);
                if (remoteCommits == null || remoteCommits.Count == 0)
                {
                    Logger.Log("No commits found on the remote repository.");
                    return;
                }

                // Store fetched commits locally
                foreach (var commit in remoteCommits)
                {
                    string commitPath = Path.Combine(Paths.CommitDir, branchName, commit.Commit);
                    if (!File.Exists(commitPath))
                    {
                        File.WriteAllText(commitPath, JsonSerializer.Serialize(commit));
                        Logger.Log($"Fetched commit {commit.Commit}.");
                    }
                }

                Logger.Log("Fetch operation completed successfully.");


            }
        }




        public class PullCommand : BaseCommand
        {
            public PullCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "pull";
            public override string Description => "pull the remote repo";
            public override async Task Execute(string[] args)
            {
                // Load credentials
                var credManager = new CredentialManager();
                var credentials = credManager.LoadCredentials();
                if (credentials == null)
                {
                    Logger.Log("Please login first. janus login");
                    return;
                }

                // Check the user is in a valid dir (.janus exists)
                if (Directory.Exists(Paths.JanusDir))
                {
                    Logger.Log("Local repository not found");
                    return;
                }

                // Get the repo name from folder
                string repoName = new DirectoryInfo(Paths.WorkingDir).Name;
                if (BranchHelper.IsValidRepoOrBranchName(repoName))
                {
                    Logger.Log($"Invalid repository name: {repoName}");
                    return;
                }

                // Get the current branch
                string branchName = MiscHelper.GetCurrentBranchName(Paths);



                // Get the local latest commit hash

                // Get the remote latest commit hash



                // Compare local and remote commits
                // if local and remote are same -> do nothing
                // if remote is ahead -> fetch and merge new commits into local
                // if local is ahead -> 

                // Get the new commits

                // Apply commits in the order they were made

            }
        }














        public class InitCommand : BaseCommand
        {
            public InitCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "init";
            public override string Description => "Initializes the janus repository.";
            public override async Task Execute(string[] args)
            {
                // Check the store user credentials
                var credManager = new CredentialManager();
                var credentials = credManager.LoadCredentials();
                if (credentials == null)
                {
                    Logger.Log("Please login first. Usage: janus login");
                    return;
                }

                // Initialise .janus folder
                if (Directory.Exists(Paths.JanusDir))
                {
                    Logger.Log("Repository already initialized");
                    return;
                }


                try
                {
                    InitHelper.InitRepo(Paths);

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

                    var fullPath = Path.Combine(Paths.WorkingDir, relativeFilePath);

                    FileMetadata metadata = GetFileMetadata(fullPath, fileHash);


                    // If the file isnt already staged or the file has been modified stage it
                    if (!stagedFiles.ContainsKey(relativeFilePath) || stagedFiles[relativeFilePath].Hash != fileHash)
                    {
                        // Write file content to objects directory
                        string objectFilePath = Path.Combine(Paths.ObjectDir, fileHash);
                        if (!File.Exists(objectFilePath)) // Dont rewrite existing objects (this way they can be reused)
                        {
                            File.WriteAllBytes(objectFilePath, content);
                        }

                        stagedFiles[relativeFilePath] = metadata;
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
                    stagedFiles[relativeFilePath] = new FileMetadata { 
                        Hash = "Deleted",
                        MimeType = null,
                        Size = 0
                    };

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

                // Get parent commit and its tree
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
                    var updatedIndex = stagedFiles.Where(kv => kv.Value.Hash != "Deleted")
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


                    string commitHash = HashHelper.ComputeCommitHash(parentCommit, branch, username, email, DateTime.UtcNow, commitMessage, rootTreeHash);

                    CommitHelper.SaveCommit(Paths, commitHash, parentCommit, branch, username, email, DateTime.UtcNow, commitMessage, rootTreeHash);

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
                            (string.IsNullOrEmpty(filters.Since) || metadata.Date >= DateTime.Parse(filters.Since)) &&
                            (string.IsNullOrEmpty(filters.Until) || metadata.Date <= DateTime.Parse(filters.Until))

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
                if (!BranchHelper.IsValidRepoOrBranchName(branchName))
                {
                    Logger.Log($"Invalid branch name: {branchName}");
                    return;
                }


                string branchPath = Path.Combine(Paths.BranchesDir, branchName, "head");

                if (File.Exists(branchPath))
                {
                    Logger.Log($"Branch '{branchName}' already exists.");
                    return;
                }


                // Get latest commit of the current branch
                string branchHeadCommit = MiscHelper.GetCurrentHeadCommitHash(Paths);

                // Create branches file for main
                var branch = new Branch
                {
                    Name = branchName,
                    SplitFromCommit = branchHeadCommit,
                    CreatedBy = MiscHelper.GetUsername(),
                    ParentBranch = MiscHelper.GetCurrentBranchName(Paths),
                    Created = DateTime.UtcNow
                };

                string branchJson = JsonSerializer.Serialize(branch, new JsonSerializerOptions { WriteIndented = true });


                string branchFolderPath = Path.Combine(Paths.BranchesDir, branchName);
                Directory.CreateDirectory(branchFolderPath);
                File.WriteAllText(Path.Combine(branchFolderPath, "info"), branchJson);


                // Put the latest commit into the new branch head
                File.WriteAllText(branchPath, branchHeadCommit);



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
                string[] allBranchesPaths = Directory.GetDirectories(Paths.BranchesDir);

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
                string branchPath = Path.Combine(Paths.BranchesDir, branchName, "head");

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
