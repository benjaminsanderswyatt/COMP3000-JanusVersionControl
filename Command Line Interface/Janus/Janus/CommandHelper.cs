using Janus.Models;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Janus
{
    internal class CommandHelper
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













        // Login / Logout
        public static string ReadSecretInput()
        {
            string input = "";
            ConsoleKeyInfo key;

            while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
            {
                input += key.KeyChar;
            }

            Console.WriteLine();
            return input;

        }

        public static string RetreiveToken()
        {
            if (!File.Exists(Paths.TokenDir))
            {
                return null;
            }

            return File.ReadAllText(Paths.TokenDir);
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

        public static string GetCurrentHead()
        {
            string headPath = Paths.head;

            if (File.Exists(headPath))
            {
                return File.ReadAllText(headPath).Trim();
            }

            return string.Empty; // Empty means this is the initial commit
        }




        public static string GenerateCommitMetadata(string commitHash, Dictionary<string, string> fileHashes, string commitMessage, string parentCommit)
        {
            var metadata = new CommitMetadata
            {
                Commit = commitHash,
                Parent = parentCommit,
                Date = DateTime.UtcNow,
                Message = commitMessage,
                Files = fileHashes
            };

            string metadataJson = JsonConvert.SerializeObject(metadata);

            return metadataJson;
        }

    }
}
