using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Janus.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace Janus
{
    internal class PushHelper
    {

        private class CommitDto
        {
            public string BranchName { get; set; }
            public string CommitHash { get; set; }
            public string Message { get; set; }
            public string ParentCommitHash { get; set; }
            public DateTime CommittedAt { get; set; }
            public List<FileDto> Files { get; set; }
        }

        private class FileDto
        {
            public string FilePath { get; set; }
            public string FileHash { get; set; }
            public byte[] FileContent { get; set; }
        }


        public static async Task<string> GetCommitMetadataFiles()
        {
            string commitDir = Paths.commitDir;

            var commits = Directory.EnumerateFiles(commitDir);


            var commitList = new List<CommitDto>();

            foreach (var commit in commits)
            {
                // Read the commit
                string commitJson = File.ReadAllText(commit);
                CommitMetadata commitMetadata = JsonConvert.DeserializeObject<CommitMetadata>(commitJson);

                var fileDtos = new List<FileDto>();
                foreach (var file in commitMetadata.Files)
                {
                    string filePath = file.Key;
                    string fileHash = file.Value;

                    string fileLocation = Path.Combine(Paths.objectDir, fileHash);
                    byte[] fileContent = await File.ReadAllBytesAsync(fileLocation);

                    fileDtos.Add(new FileDto
                    {
                        FilePath = filePath,
                        FileHash = fileHash,
                        FileContent = fileContent
                    });
                }


                // Create new commit object
                commitList.Add(new CommitDto
                {
                    CommitHash = commitMetadata.Commit,
                    ParentCommitHash = commitMetadata.Parent,
                    CommittedAt = commitMetadata.Date,
                    Message = commitMetadata.Message,
                    Files = fileDtos
                });

            }

            return JsonConvert.SerializeObject(commitList);
        }



        // Auth header


        

        public static async Task<string> PostToBackendAsync(string commitJson)
        {
            string apiUrl = "https://localhost:82/api/CLI/Push";


            using (HttpClient client = new HttpClient())
            {
                // Authorization
                string accessToken = CommandHelper.RetreiveToken();
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




    }
}
