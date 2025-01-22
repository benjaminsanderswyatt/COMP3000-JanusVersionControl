using Janus.Models;
using Janus.Plugins;
using System.Text;
using System.Text.Json;

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


        public static bool ConfirmAction(ILogger logger, string message)
        {
            while (true)
            {
                logger.Log($"{message} (Y/N)");
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Y)
                {
                    logger.Log("");
                    return true;
                }
                else if (key.Key == ConsoleKey.N)
                {
                    logger.Log("");
                    return false;
                }
                else
                {
                    logger.Log("");
                    logger.Log("Invalid input. Please confirm 'Y' or 'N'.");
                }
            }
        }





        public static string GetUsername()
        {
            // Get the username from configs
            // TODO

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

        public static string GetCurrentBranchName(Paths paths)
        {
            // Get the current branch name
            return GetCurrentBranchRelPath(paths).Split('/').Last();
        }




        public static string GetCurrentHEAD(Paths paths)
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


        public static string GenerateCommitMetadata(string branch, string commitHash, string treeHash, string commitMessage, string parentCommit, string author)
        {
            var metadata = new CommitMetadata
            {
                Commit = commitHash,
                Parent = parentCommit,
                Branch = branch,
                Author = author,
                Date = DateTimeOffset.Now,
                Message = commitMessage,
                Tree = treeHash
            };

            string metadataJson = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });

            return metadataJson;
        }



        public static void DisplaySeperator(ILogger logger)
        {
            logger.Log(new string('-', 50));
        }





    }
}
