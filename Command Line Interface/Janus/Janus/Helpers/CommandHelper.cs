using Janus.Models;
using Janus.Plugins;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Janus.Helpers
{
    public class CommandHelper
    {
        public static async Task ExecuteAsync()
        {
            string apiUrl = "https://localhost:82/api/Test/SayHello";

            // The data you want to send
            string testMessage = "Hello from the console app!";

            using (HttpClient client = new HttpClient())
            {
                // Serialize the string to JSON
                StringContent content = new StringContent(
                    $"\"{testMessage}\"",
                    Encoding.UTF8,
                    "application/json");

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
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Request error: {ex.Message}");
                }
            }
        }


        public static string GetUsername()
        {
            // Get the username from configs


            // No config set use systems username
            if (string.IsNullOrWhiteSpace(Environment.UserName))
            {
                return "unknown";
            }

            return Environment.UserName;
        }
        









        public static bool ValidateRepoExists(ILogger Logger, Paths paths)
        {
            if (!Directory.Exists(paths.JanusDir))
            {
                Logger.Log("Not a janus repository. Use 'init' command to initialise repository.");

                return false;
            }

            return true;
        }




        public static string ComputeHash(string content)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                byte[] hashBytes = sha1.ComputeHash(contentBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "");
            }
        }

        public static string ComputeCommitHash(Dictionary<string, string> fileHashes, string commitMessage)
        {
            string combined = string.Join("", fileHashes.Select(kv => kv.Key + kv.Value)) + commitMessage;

            return ComputeHash(combined);
        }



        public static string GetCurrentBranchRelPath(Paths paths)
        {
            // Ensure the HEAD file exists
            if (!File.Exists(paths.HEAD))
            {
                throw new FileNotFoundException("HEAD file not found. The repository may not be initialized correctly.", paths.HEAD);
            }

            // Check contents of HEAD
            var pointer = File.ReadAllText(paths.HEAD).Trim();

            if (string.IsNullOrWhiteSpace(pointer))
            {
                throw new InvalidDataException("HEAD file is empty or invalid.");
            }

            if (!pointer.StartsWith("ref: "))
            {
                throw new InvalidDataException("HEAD file does not point to a valid reference.");
            }

            // Get the ref path from the pointer
            var refPath = pointer.Substring(5); // Remove "ref: "

            return refPath;
        }





        public static string GetCurrentHead(Paths paths)
        {
            // Ensure the HEAD file exists
            if (!File.Exists(paths.HEAD))
            {
                throw new FileNotFoundException("HEAD file not found. The repository may not be initialized correctly.", paths.HEAD);
            }

            // Check contents of HEAD
            string refPath = GetCurrentBranchRelPath(paths);

            var fullRefPath = Path.Combine(paths.JanusDir, refPath);

            // Ensure the ref file exists
            if (!File.Exists(fullRefPath))
            {
                throw new FileNotFoundException($"Reference file not found for {refPath}.", fullRefPath);
            }

            // Read the commit hash from the ref file
            var commitHash = File.ReadAllText(fullRefPath).Trim();

            if (commitHash == null)
            {
                throw new InvalidDataException("Reference file contains an invalid commit hash.");
            }

            return commitHash;
        }




        public static string GenerateCommitMetadata(string branch, string commitHash, Dictionary<string, string> fileHashes, string commitMessage, string parentCommit, string author)
        {
            var metadata = new CommitMetadata
            {
                Commit = commitHash,
                Parent = parentCommit,
                Branch = branch,
                Author = author,
                Date = DateTimeOffset.Now,
                Message = commitMessage,
                Files = fileHashes
            };

            string metadataJson = JsonConvert.SerializeObject(metadata, Formatting.Indented);

            return metadataJson;
        }

    }
}
