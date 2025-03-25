using Janus.API;
using Janus.DataTransferObjects;
using Janus.Helpers;
using Janus.Helpers.CommandHelpers;
using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Security.AccessControl;
using System.Text.Json;
using static Janus.Diff;
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

                new ConfigCommand(logger, paths),
                new MergeCommand(logger, paths),

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


        public class ConfigCommand : BaseCommand
        {
            public ConfigCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "config";
            public override string Description => "Manages local and global configuration settings";
            public override string Usage =>
@"janus config <subcommand> [arguments]
Subcommands:
    ip get [--global]           : Gets the configured IP (local or global)
    ip set <value> [--global]   : Sets the IP configuration
    ip reset [--global]         : Removes configuration file
Examples:
    janus config ip get
    janus config ip set 192.168.1.100 --global
    janus config ip reset          : Removes local config
    janus config ip reset --global : Removes global config";
            public override async Task Execute(string[] args)
            {
                if (args.Length < 2)
                {
                    Logger.Log("Please specify a subcommand");
                    return;
                }

                // Check for the --global flag
                bool isGlobal = args.Any(arg => arg.Equals("--global", StringComparison.OrdinalIgnoreCase));

                // Determine the proper config file path based on the flag
                string configPath = isGlobal ? Paths.GlobalConfig : Paths.LocalConfig;

                var configManager = new ConfigManager(Paths.LocalConfig, Paths.GlobalConfig);

                if (args[0].ToLower() == "ip")
                {

                    switch (args[1].ToLower())
                    {
                        case "get":

                            try
                            {
                                string ip = configManager.GetIp(isGlobal);

                                if (string.IsNullOrWhiteSpace(ip))
                                {
                                    Logger.Log("No IP configuration found");
                                }
                                else
                                {
                                    Logger.Log("Configured IP: " + ip);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Log("Error reading IP configuration: " + ex.Message);
                            }

                            break;

                        case "set":

                            if (args.Length < 3)
                            {
                                Logger.Log("Please provide an IP");
                                return;
                            }
                            string newIP = args[2];
                            try
                            {
                                configManager.SetIp(isGlobal, newIP);
                                Logger.Log("IP configuration set to: " + newIP);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log("Error setting IP configuration: " + ex.Message);
                            }

                            break;

                        case "reset":

                            try
                            {
                                configManager.ResetIp(isGlobal);
                                Logger.Log("IP configuration has been reset");
                            }
                            catch (Exception ex)
                            {
                                Logger.Log("Error resetting IP configuration: " + ex.Message);
                            }

                            break;


                        default:
                            Logger.Log("Invalid subcommand");
                            break;
                    }

                }
                else
                {
                    Logger.Log("Invalid command. Use 'janus config <subcommand>'");
                }






            }

        }






        public class MergeCommand : BaseCommand
        {
            public MergeCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "merge";
            public override string Description => "Merge changes from another branch into the current";
            public override string Usage =>
@"janus merge <branch>
<branch>: Branch to merge into the current branch
Example:
    janus merge featureBranch";
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
                if (!Directory.Exists(Paths.JanusDir))
                {
                    Logger.Log("Local repository not found");
                    return;
                }

                if (args.Length == 0)
                {
                    Logger.Log("Please provide a branch to merge");
                    return;
                }

                string targetBranch = args[0];


                try
                {
                    // Check target branch exists
                    string targetBranchHead = Path.Combine(Paths.BranchesDir, targetBranch, "head");

                    if (!File.Exists(targetBranchHead))
                    {
                        Logger.Log($"Branch '{targetBranch}' doesnt exist");
                        return;
                    }


                    // Get the current head and branch head
                    string currentBranch = MiscHelper.GetCurrentBranchName(Paths);
                    string currentHead = BranchHelper.GetBranchHead(Paths, currentBranch);
                    string targetHead = BranchHelper.GetBranchHead(Paths, targetBranch);


                    // Check if the branches are the same
                    if (currentHead == targetHead)
                    {
                        Logger.Log("No changes to merge");
                        return;
                    }


                    // Get the common ancestor
                    string commonAncestor = MergeHelper.FindCommonAncestor(Logger, Paths, currentHead, targetHead);

                    if (commonAncestor == null)
                    {
                        Logger.Log($"No common ancestor found. Cannot merge");
                        return;
                    }



                    // Get trees for comparison
                    TreeNode baseTree = Diff.GetTreeFromCommit(Logger, Paths, commonAncestor);
                    TreeNode currentTree = Diff.GetTreeFromCommit(Logger, Paths, currentHead);
                    TreeNode targetTree = Diff.GetTreeFromCommit(Logger, Paths, targetHead);
                    

                    // Compute merge changes
                    var mergeResult = MergeHelper.ComputeMergeChanges(Logger, Paths, baseTree, currentTree, targetTree);


                    // Handle conflicts
                    if (mergeResult.HasConflicts)
                    {
                        Logger.Log("Merge conflicts detected:");
                        foreach (var conflict in mergeResult.Conflicts)
                        {
                            Logger.Log($"Conflict: {conflict}");
                        }
                        Logger.Log("Resolve conflificts and commit the result");
                        return;
                    }

                    // Create merge commit
                    string commitMessage = $"Merge branch '{targetBranch}' into '{currentBranch}'";

                    var (newTreeHash, tree) = TreeBuilder.CreateMergedTree(Paths, mergeResult.MergedEntries);


                    // Create commit with 2 parents
                    string commitHash = CommitHelper.CreateMergeCommit(
                        Paths,
                        currentHead,
                        targetHead,
                        newTreeHash,
                        commitMessage,
                        MiscHelper.GetUsername(),
                        MiscHelper.GetEmail()
                    );


                    // Update branch head
                    HeadHelper.SetHeadCommit(Paths, commitHash, currentBranch);

                    // Recreate working directory
                    MergeHelper.RecreateWorkingDir(Logger, Paths, tree);

                    Logger.Log($"Created merge commit {commitHash}");

                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to merge: {ex.Message}");
                }



            }
        }
        




        public class DiffCommand : BaseCommand
        {
            public DiffCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "diff";
            public override string Description => "Displays differences between versions.";
            public override string Usage =>
@"janus diff [options] [<commit> [<commit>]] [--path <file>]
Options:
    --staged         : Show changes staged for commit.
    --path <file>    : Filter diff to a specific file.
    --parent         : Compare a commit with its parent.
Examples:
    janus diff                     : Show unstaged changes.
    janus diff --staged            : Show staged changes.
    janus diff abc123 def456       : Compare two commits.
    janus diff abc123 def456 --path file.txt : Compare a specific file between commits.
    janus diff abc123 --parent     : Compare a commit with its parent.";
            public override async Task Execute(string[] args)
            {
                bool staged = false;
                string pathFilter = null;
                bool parent = false;
                List<string> commits = new List<string>();

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--staged")
                    {
                        staged = true;
                    }
                    else if (args[i] == "--path")
                    {
                        if (i + 1 < args.Length)
                        {
                            pathFilter = args[++i].Replace('/', Path.DirectorySeparatorChar);
                        }
                    }
                    else if (args[i] == "--parent")
                    {
                        parent = true;
                    }
                    else
                    {
                        commits.Add(args[i]);
                    }
                }
                



                // Determine comparison mode
                TreeNode oldTree = null;
                TreeNode newTree = null;
                TreeSourceType oldSourceType = TreeSourceType.Staged;
                TreeSourceType newSourceType = TreeSourceType.Staged;

                if (staged)
                {
                    oldTree = Tree.GetHeadTree(Logger, Paths);
                    newTree = Tree.GetStagedTree(Paths);
                    oldSourceType = TreeSourceType.Commit;
                    newSourceType = TreeSourceType.Staged;
                }
                else if (commits.Count == 2)
                {
                    oldTree = GetTreeFromCommit(Logger, Paths, commits[0]);
                    newTree = GetTreeFromCommit(Logger, Paths, commits[1]);
                    oldSourceType = TreeSourceType.Commit;
                    newSourceType = TreeSourceType.Commit;
                }
                else if (commits.Count == 1 && parent)
                {
                    string commit = commits[0];
                    string parentCommit = GetParentCommit(Logger, Paths, commit);

                    if (string.IsNullOrEmpty(parentCommit))
                    {
                        oldTree = new TreeNode("root");
                    }
                    else
                    {
                        oldTree = GetTreeFromCommit(Logger, Paths, parentCommit);
                    }

                    newTree = GetTreeFromCommit(Logger, Paths, commit);
                    oldSourceType = TreeSourceType.Commit;
                    newSourceType = TreeSourceType.Commit;
                }
                else
                {
                    oldTree = Tree.GetHeadTree(Logger, Paths);
                    newTree = Tree.GetWorkingTree(Paths);
                    oldSourceType = TreeSourceType.Commit;
                    newSourceType = TreeSourceType.Working;
                }


                if (oldTree == null || newTree == null)
                {
                    Logger.Log("Error: Getting trees for comparison");
                    return;
                }
                

                // Compare trees
                var comparisonResult = Tree.CompareTrees(oldTree, newTree);

                // Filter by path
                if (!string.IsNullOrEmpty(pathFilter))
                {
                    comparisonResult.AddedOrUntracked = comparisonResult.AddedOrUntracked
                        .Where(p => p == pathFilter).ToList();

                    comparisonResult.ModifiedOrNotStaged = comparisonResult.ModifiedOrNotStaged
                        .Where(p => p == pathFilter).ToList();

                    comparisonResult.Deleted = comparisonResult.Deleted
                        .Where(p => p == pathFilter).ToList();
                }

                // Display diff
                DisplayDiffs(Logger, Paths, comparisonResult, oldTree, newTree, oldSourceType, newSourceType);

            }
        }






        public class HelpCommand : BaseCommand
        {
            public HelpCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "help";
            public override string Description => "Displays all available commands or detailed usage for a specific command";
            public override string Usage =>
@"janus help [command]
    [command] : (Optional) The command for which detailed usage is displayed.
Examples:
    janus help
    janus help clone";
            public override async Task Execute(string[] args)
            {
                if (args.Length == 0)
                {
                    Logger.Log("Usage: janus <command> [arguments]");
                    Logger.Log("Commands:");
                    foreach (var command in Program.CommandList)
                    {
                        Logger.Log($"{command.Name.PadRight(20)} : {command.Description}");
                    }

                    Logger.Log("\nFor detailed usage of a specific command, type:");
                    Logger.Log("  janus help [command]");
                }
                else
                {

                    // If a specific command is provided display its usage and description
                    string commandName = args[0];
                    var command = Program.CommandList
                        .FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

                    if (command != null)
                    {
                        Logger.Log($"Usage for command '{command.Name}':");
                        Logger.Log($"{command.Usage}");
                        Logger.Log($"{command.Description}");
                    }
                    else
                    {
                        Logger.Log($"Command '{commandName}' not found.");
                    }

                }

            }
        }

        public class LoginCommand : BaseCommand
        {
            public LoginCommand(ILogger logger, Paths paths) : base(logger, paths) { }

            public override string Name => "login";
            public override string Description => "Saves your user credentials for repository access.";
            public override string Usage =>
@"janus login
This command prompts you for:
    Username : Your user name.
    Email    : Your email address.
    Token    : (Optional) Your personal access token.
Example:
    janus login";
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
            public override string Description => "Manages remote repository settings (add, remove, list).";
            public override string Usage =>
@"janus remote <subcommand> [arguments]
Subcommands:
    add <name> <endpoint>    : Adds a remote repository.
    remove <name>        : Removes a remote repository.
    list                 : Lists all remote repositories.
Example:
    janus remote add origin janus/repo/user";
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
            public override string Description => "Clones a repository from a remote server to your local machine.";
            public override string Usage =>
@"janus clone <endpoint> [branch]
<endpoint> : Repository endpoint in the format 'janus/{owner}/{repoName}'
[branch]   : (Optional) Branch to activate (defaults to 'main')
Example:
    janus clone janus/owner/repo main";
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
                    var (success, data) = await ApiHelper.SendGetAsync(Paths, endpoint, credentials.Token);

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


                    // Check the chosen branch exists
                    if (!cloneData.Branches.Any(b => b.BranchName == chosenBranch))
                    {
                        // Default main and inform user
                        chosenBranch = "main";
                        Logger.Log($"Your chosen branch doesnt exist. Defaulting to main...");
                    }


                    Logger.Log($"Setting repository configs...");
                    // Create repo config file
                    MiscHelper.CreateRepoConfig(clonePaths.LocalConfig, cloneData.IsPrivate, cloneData.RepoDescription);

                    
                    // Get file hashes from commits
                    var fileHashes = new HashSet<string>();
                    var branchTrees = new Dictionary<string, TreeNode>();

                    foreach (var branch in cloneData.Branches)
                    {
                        Logger.Log($"Building '{branch.BranchName}' branch...");

                        TreeNode latestTree = null;

                        foreach (var commit in branch.Commits)
                        {

                            // Save commit
                            CommitHelper.SaveCommit(clonePaths,
                                commit.CommitHash,
                                commit.ParentsCommitHash,
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


                                // Store latest tree for branch
                                latestTree = treeBuilder.root;

                                // Get file hashes from tree
                                treeBuilder.GetFileHashes(fileHashes);

                            }
                        }

                        if (latestTree != null)
                        {
                            branchTrees[branch.BranchName] = latestTree;
                        }
                    }


                    // Create branch dirs and files
                    foreach (var branchDto in cloneData.Branches)
                    {
                        string branchDir = Path.Combine(clonePaths.BranchesDir, branchDto.BranchName);
                        Directory.CreateDirectory(branchDir);

                        // Create head file with latest commit
                        File.WriteAllText(Path.Combine(branchDir, "head"), branchDto.LatestCommitHash);

                        // Create info file
                        var branchInfo = new Branch
                        {
                            Name = branchDto.BranchName,
                            SplitFromCommit = branchDto.SplitFromCommitHash,
                            ParentBranch = branchDto.ParentBranch,
                            CreatedBy = branchDto.CreatedBy,
                            Created = branchDto.Created
                        };

                        File.WriteAllText(Path.Combine(branchDir, "info"),
                            JsonSerializer.Serialize(branchInfo, new JsonSerializerOptions { WriteIndented = true }));
                    
                        // Create branch index from latest tree
                        if (branchTrees.TryGetValue(branchDto.BranchName, out var tree))
                        {
                            var indexDict = new TreeBuilder(clonePaths).BuildIndexDictionary(tree);
                            IndexHelper.SaveIndex(Path.Combine(branchDir, "index"), indexDict);
                        }
                        else
                        {
                            File.Create(Path.Combine(branchDir, "index")).Close();
                        }

                    }

                    // Set active branch
                    string activeBranchDir = Path.Combine(clonePaths.BranchesDir, chosenBranch);
                    
                    // Copy active branch's index to main index
                    var activeIndex = IndexHelper.LoadIndex(Path.Combine(activeBranchDir, "index"));
                    IndexHelper.SaveIndex(clonePaths.Index, activeIndex);

                    // Set HEAD reference
                    BranchHelper.SetCurrentHEAD(clonePaths, chosenBranch);



                    // Download the file objects
                    Logger.Log($"Downloading file data...");

                    if (fileHashes.Any())
                    {
                        bool downloadSuccess = await ApiHelper.DownloadBatchFilesAsync(
                            Paths,
                            owner,
                            repoName,
                            fileHashes.ToList(),
                            clonePaths.ObjectDir,
                            credentials.Token
                        );


                        if (!downloadSuccess)
                        {
                            Logger.Log("Error downloading file objects");
                            Directory.Delete(repoPath, true); // Clean up partial clone
                            return;
                        }
                    }


                    // Recreate working directory
                    Logger.Log("Recreating working directory files...");
                    if (branchTrees.TryGetValue(chosenBranch, out var activeTree))
                    {
                        MergeHelper.RecreateWorkingDir(Logger, clonePaths, activeTree);
                    }


                    Logger.Log($"Repository '{repoName}' successfully cloned to '{repoPath}'");

                }
                catch (Exception ex)
                {
                    Logger.Log($"An error occurred during cloning: {ex.Message}");

                    if (Directory.Exists(repoPath)) // Cleanup fail
                    {
                        Directory.Delete(repoPath, true);
                    }
                }


            }

        }









        public class PushCommand : BaseCommand
        {
            public PushCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "push";
            public override string Description => "Pushes your local commits to the remote repository.";
            public override string Usage =>
@"janus push
Ensure:
    - You are in a valid repository.
    - All changes have been committed.
    - You are logged in.
Example:
    janus push";
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
                var (success, response) = await ApiHelper.SendPostAsync(Paths, $"Push/{repoName}/{branchName}", pushData, credentials.Token);

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
            public override string Description => "Fetches the latest commits from the remote repository and updates your local history.";
            public override string Usage =>
@"janus fetch
This command will:
    - Retrieve new commits from the remote repository.
    - Update your local commit history.
Ensure you are logged in and in a valid repository.
Example:
    janus fetch";
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
                var (success, remoteCommitsJson) = await ApiHelper.SendGetAsync(Paths, $"{owner}/{repoName}/{branchName}/commits", credentials.Token);
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
            public override string Description => "Pulls changes from the remote repository and updates your local working directory.";
            public override string Usage =>
@"janus pull
This command will:
    - Fetch new commits from the remote.
    - Merge remote changes into your current branch.
Notes:
    - Ensure all local changes are committed or stashed.
Example:
    janus pull";
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
            public override string Description => "Initialises the janus repository.";
            public override string Usage =>
@"janus init
This command will:
    - Create the .janus directory and subdirectories.
    - Set up initial configuration files.
Prerequisite:
    - You must be logged in (use 'janus login').
Example:
    janus init";
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
                    Logger.Log("Repository already initialised");
                    return;
                }


                try
                {
                    InitHelper.InitRepo(Paths);

                    Logger.Log("Initialised janus repository");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error initialising repository: {ex.Message}");
                }
            }
        }



        public class AddCommand : BaseCommand
        {
            public AddCommand(ILogger logger, Paths paths) : base(logger, paths) { }
            public override string Name => "add";
            public override string Description => "Stages specified files or directories for the next commit.";
            public override string Usage =>
@"janus add <file(s) or directory>
Examples:
    janus add file.txt           : Stages a single file.
    janus add folder             : Stages all files within a folder.
    janus add --all              : Stages every file in the repository.
Notes:
    - You can provide relative or absolute paths.
    - '--all' stages the entire working directory.";
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

                if (args.Any(arg => arg.ToLowerInvariant().Equals("--all", StringComparison.OrdinalIgnoreCase)))
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
            public override string Description => "Commits the staged changes to the repository with a commit message.";
            public override string Usage =>
@"janus commit <commit message>
<commit message> : A short description of the changes.
Notes:
    - Ensure you have staged files using 'janus add'.
    - The commit message should be descriptive.
Example:
    janus commit ""Fixed bug in file upload""";
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

                    CommitHelper.SaveCommit(Paths, commitHash, new List<string> { parentCommit }, branch, username, email, DateTime.UtcNow, commitMessage, rootTreeHash);

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
            public override string Description => "Displays the commit history. Optional filters allow you to narrow down the results.";
            public override string Usage =>
@"janus log [options]
Options:
    --branch <branch>   : Show commits from a specific branch.
    --author <author>   : Show commits by a specific author.
    --since <date>      : Show commits since the specified date (YYYY-MM-DD).
    --until <date>      : Show commits until the specified date (YYYY-MM-DD).
    --limit <number>    : Limit the number of commits displayed.
Example:
    janus log --branch main --author Alice --since 2023-01-01 --limit 10";
            public override async Task Execute(string[] args)
            {
                // Repository has to be initialised for command to run
                if (!MiscHelper.ValidateRepoExists(Logger, Paths)) { return; }

                if (!Directory.Exists(Paths.CommitDir))
                {
                    Logger.Log("Error: Commit directory doesn't exist.");
                    return;
                }

                // Get all commit files
                var commitFiles = Directory.GetFiles(Paths.CommitDir);

                if (commitFiles.Length == 0)
                {
                    Logger.Log("Error: No commits found. Repository might not have been initialised correctly.");
                    return;
                }

                // Parse options using CLI notation
                string branch = null, author = null, since = null, until = null, limit = null;
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "--branch":
                            if (i + 1 < args.Length)
                            {
                                branch = args[++i];
                            }
                            break;
                        case "--author":
                            if (i + 1 < args.Length)
                            {
                                author = args[++i];
                            }
                            break;
                        case "--since":
                            if (i + 1 < args.Length)
                            {
                                since = args[++i];
                            }
                            break;
                        case "--until":
                            if (i + 1 < args.Length)
                            {
                                until = args[++i];
                            }
                            break;
                        case "--limit":
                            if (i + 1 < args.Length)
                            {
                                limit = args[++i];
                            }
                            break;
                    }
                }

                LogFilters filters = new LogFilters
                {
                    Branch = branch,
                    Author = author,
                    Since = since,
                    Until = until,
                    Limit = limit
                };

                List<CommitMetadata?> commitPathsInFolder;
                try
                {
                    commitPathsInFolder = commitFiles
                        .Select(commit => JsonSerializer.Deserialize<CommitMetadata>(File.ReadAllText(commit)))
                        .Where(metadata => metadata != null)
                        .Where(metadata =>
                            (string.IsNullOrEmpty(filters.Branch) || metadata.Branch.Equals(filters.Branch, StringComparison.OrdinalIgnoreCase)) &&
                            (string.IsNullOrEmpty(filters.Author) || metadata.AuthorName.Equals(filters.Author, StringComparison.OrdinalIgnoreCase)) &&
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
                if (int.TryParse(filters.Limit, out int limitValue))
                {
                    commitPathsInFolder = commitPathsInFolder.Take(limitValue).ToList();
                }

                // Display commit history
                if (!commitPathsInFolder.Any())
                {
                    Logger.Log("No commits match the provided filters.");
                    return;
                }

                foreach (var commit in commitPathsInFolder)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Logger.Log($"Commit:  {commit.Commit}");

                    Console.ForegroundColor = ConsoleColor.Green;
                    Logger.Log($"Author:  {commit.AuthorName} ({commit.AuthorEmail})");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Logger.Log($"Date:    {commit.Date}");

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Logger.Log($"Branch:  {commit.Branch}");

                    Console.ForegroundColor = ConsoleColor.White;
                    Logger.Log($"Message: {commit.Message}");

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
            public override string Description => "Creates a new branch based on the current HEAD commit.";
            public override string Usage =>
@"janus create_branch <branch name>
<branch name> : Name of the new branch to create.
This command:
    - Validates the branch name.
    - Sets the new branch's HEAD to the current commit.
Example:
    janus create_branch featureBranch";
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
            public override string Description => "Lists all branches in the repository and highlights the current branch.";
            public override string Usage =>
@"janus list_branch
This command displays:
    - All branch names.
    - The current branch marked with an arrow (->).";
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
            public override string Description => "Switches the working directory to a different branch.";
            public override string Usage =>
@"janus switch_branch <branch name> [--force]
<branch name> : The branch to switch to.
--force       : (Optional) Force the switch even if there are uncommitted changes (they will be lost).
Example:
    janus switch_branch develop
    janus switch_branch featureBranch --force";
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
            public override string Description => "Displays the repository status including branch info, staged changes, and untracked files.";
            public override string Usage =>
@"janus status
This command shows:
    - The current branch.
    - Files staged for commit.
    - Modified files not staged.
    - Untracked files.
    - A summary of local vs remote changes.
Example:
    janus status";
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
