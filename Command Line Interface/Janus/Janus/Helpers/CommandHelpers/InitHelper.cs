using Janus.API;
using Janus.Helpers;
using Janus.Models;
using Janus.Plugins;
using Janus.Utils;
using System.Text.Json;

namespace Janus.Helpers.CommandHelpers
{
    public class InitHelper
    {

        public static async void InitRepo(Paths paths, bool doInitCommit = true)
        {
            try
            {
                Directory.CreateDirectory(paths.JanusDir);
                File.SetAttributes(paths.JanusDir, File.GetAttributes(paths.JanusDir) | FileAttributes.Hidden); // Makes the janus folder hidden


                Directory.CreateDirectory(paths.ObjectDir); // .janus/objects folder
                Directory.CreateDirectory(paths.TreeDir); // .janus/trees folder
                Directory.CreateDirectory(paths.HeadsDir); // .janus/heads
                Directory.CreateDirectory(paths.PluginsDir); // .janus/plugins folder
                Directory.CreateDirectory(paths.CommitDir); // .janus/commits folder
                Directory.CreateDirectory(paths.BranchesDir); // .janus/branches folder

                // Create remote file
                File.WriteAllText(paths.Remote, "[]");



                // Create index file
                File.Create(paths.Index).Close();

                // Create HEAD file pointing at main branch
                File.WriteAllText(paths.HEAD, "ref: heads/main");

                var initCommitHash = "";

                if (doInitCommit)
                {
                    // Create initial commit
                    var (returnedCommitHash, commitMetadata) = MiscHelper.CreateInitData();

                    initCommitHash = returnedCommitHash;

                    // Save the commit object in the commit directory
                    string commitFilePath = Path.Combine(paths.CommitDir, initCommitHash);
                    File.WriteAllText(commitFilePath, commitMetadata);
                }
                

                // Create main branch in heads/ pointing to initial commit
                File.WriteAllText(Path.Combine(paths.HeadsDir, "main"), initCommitHash);

                // Create detached Head file
                File.WriteAllText(paths.DETACHED_HEAD, initCommitHash);




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
                string branchFolderPath = Path.Combine(paths.BranchesDir, "main");
                Directory.CreateDirectory(branchFolderPath);
                File.WriteAllText(Path.Combine(branchFolderPath, "info"), branchJson);

                File.Create(Path.Combine(branchFolderPath, "index")).Close();


                // Create config file (for private & description)
                var repoConfig = new RepoConfig
                {
                    IsPrivate = true,
                    Description = ""
                };

                string configJson = JsonSerializer.Serialize(repoConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(paths.RepoConfig, configJson);

            }
            catch (Exception ex)
            {
                // Rethrow error
                throw;
            }
        }

    }
}
