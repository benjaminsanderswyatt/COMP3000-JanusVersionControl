using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Janus.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Janus.Helpers
{
    internal class PushHelper
    {

        private class CommitDto
        {
            public string BranchName { get; set; }
            public string CommitHash { get; set; }
            public string Message { get; set; }
            public string ParentCommitHash { get; set; }
            public DateTimeOffset CommittedAt { get; set; }
            public List<FileDto> Files { get; set; }
        }

        private class FileDto
        {
            public string FilePath { get; set; }
            public string FileHash { get; set; }
            public byte[] FileContent { get; set; }
        }

        private class RepoNameBranch
        {
            public string RepoName { get; set; }
            public string BranchName { get; set; }
        }

        private class RemoteHeadCommit
        {
            public string CommitHash { get; set; }
        }

        /*
        public static async string GetCommitMetadataFiles()
        {
            Console.WriteLine("Getting commits");

            try
            {
                string commitDir = Paths.commitDir;

                var refHead = File.ReadAllText(Paths.head).Substring(5); // Remove the "ref: " at the start

                string localHeadCommit = File.ReadAllText(Path.Combine(Paths.janusDir, refHead));

                string repoName = Path.GetFileName(Directory.GetCurrentDirectory()); // Get the name of the folder (thats the repo name)

                var repoNameBranch = new RepoNameBranch
                {
                    RepoName = repoName,
                    BranchName = refHead
                };

                var remoteHeadResponse = await GetRemoteHeadCommit(JsonSerializer.Serialize(repoNameBranch));

                var remoteHeadResponseObj = JsonSerializer.Deserialize<RemoteHeadCommit>(remoteHeadResponse);

                string remoteLatestCommitHash = remoteHeadResponseObj.CommitHash;

                string localLatestCommitHash = File.ReadAllText(Path.Combine(commitDir, localHeadCommit)); // Get the latest commit


                var commitList = new List<CommitDto>();
                string currentCommitHash = localLatestCommitHash;
                while (remoteLatestCommitHash != localLatestCommitHash) // Initial commit is when both local and remote are refs: refs/heads/"branchName"
                {
                    // Read the current commit
                    string commitJson = File.ReadAllText(Path.Combine(Paths.commitDir, currentCommitHash));

                    CommitMetadata commitMetadata = JsonSerializer.Deserialize<CommitMetadata>(commitJson);

                    var fileDtos = new List<FileDto>();
                    foreach (var file in commitMetadata.Files)
                    {
                        Console.WriteLine("File: " + file.Key + " | " + file.Value);
                        string filePath = file.Key;
                        string fileHash = file.Value;

                        string fileLocation = Path.Combine(Paths.objectDir, fileHash);
                        Console.WriteLine("Location: " + fileLocation);
                        byte[] fileContent = File.ReadAllBytes(fileLocation); //await File.ReadAllBytesAsync(fileLocation);
                        Console.WriteLine("Made it: " + fileContent);

                        fileDtos.Add(new FileDto
                        {
                            FilePath = filePath,
                            FileHash = fileHash,
                            FileContent = fileContent
                        });

                    }

                    // Create new commit object and add it to the list
                    commitList.Add(new CommitDto
                    {
                        CommitHash = commitMetadata.Commit,
                        ParentCommitHash = commitMetadata.Parent,
                        CommittedAt = commitMetadata.Date,
                        Message = commitMetadata.Message,
                        Files = fileDtos
                    });

                    // Set the current commit to the parent
                    currentCommitHash = commitMetadata.Parent;
                }

                if (commitList.Count < 1)
                {
                    return "Remote repo is up to date with local";
                }

                // Commits to be pushed
                return JsonSerializer.Serialize(commitList, new JsonSerializerOptions { WriteIndented = true });

            } 
            catch (Exception ex)
            {
                Console.WriteLine("Error getting commits: " + ex.ToString());
                return null;
            }


        }

        */


        /*
        public static async Task<string> GetRemoteHeadCommit(string repoNameBranch)
        {
            string apiUrl = "https://localhost:82/api/CLI/RemoteHeadCommit";

            Console.WriteLine("GetRemoteHeadCommit started");
            using (HttpClient client = new HttpClient())
            {
                // Authorization
                string? accessToken = CommandHelper.RetreiveToken();
                Console.WriteLine("PAT: " + accessToken);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var content = new StringContent(repoNameBranch, Encoding.UTF8, "application/json");

                try
                {
                    // Send a POST request
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    // Ensure the response was successful
                    response.EnsureSuccessStatusCode();

                    // Optionally read the response content
                    string responseContent = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("Response received:");
                    Console.WriteLine(responseContent);
                    return responseContent;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Request error: {ex.Message}");
                    return ex.ToString();
                }
            }
        }

        public static async Task<string> PostToBackendAsync(string commitJson)
        {
            string apiUrl = "https://localhost:82/api/CLI/Push";

            Console.WriteLine("Post started");
            using (HttpClient client = new HttpClient())
            {
                // Authorization
                string? accessToken = CommandHelper.RetreiveToken();
                Console.WriteLine("PAT: " + accessToken);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


                var content = new StringContent(commitJson, Encoding.UTF8, "application/json");

                try
                {
                    // Send a POST request
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    // Ensure the response was successful
                    response.EnsureSuccessStatusCode();

                    // Optionally read the response content
                    string responseContent = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("Response received:");
                    Console.WriteLine(responseContent);
                    return responseContent;
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Request error: {ex.Message}");
                    return ex.ToString();
                }
            }
        }
        */



    }
}
