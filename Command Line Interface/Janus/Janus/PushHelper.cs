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
            public DateTimeOffset CommittedAt { get; set; }
            public List<FileDto> Files { get; set; }
        }

        private class FileDto
        {
            public string FilePath { get; set; }
            public string FileHash { get; set; }
            public byte[] FileContent { get; set; }
        }

        private class TestThing
        {
            public List<FileDto> Files { get; set; }
        }


        public static string GetCommitMetadataFiles()
        {
            Console.WriteLine("Getting commits");
            try
            {
                string commitDir = Paths.commitDir;

                var commits = Directory.EnumerateFiles(commitDir);


                var commitList = new List<CommitDto>();
                var testList = new List<TestThing>();

                foreach (var commit in commits)
                {
                    Console.WriteLine("Commit foreach: " + commit);

                    // Read the commit
                    string commitJson = File.ReadAllText(commit);

                    Console.WriteLine("Read commit: " + commitJson);
                    CommitMetadata commitMetadata = JsonSerializer.Deserialize<CommitMetadata>(commitJson);
                    // Console.WriteLine($"Deserialized Date: {commitMetadata.Date}");
                    // Console.WriteLine($"Deserialized Message: {commitMetadata.Message}");

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

                        Console.WriteLine("fileList: " + fileDtos.First().FilePath);
                    }
                    Console.WriteLine("Out of loops: " + fileDtos.Count());


                    Console.WriteLine(JsonSerializer.Serialize(fileDtos));
                    
                    // Create new commit object
                    commitList.Add(new CommitDto
                    {
                        CommitHash = commitMetadata.Commit,
                        ParentCommitHash = commitMetadata.Parent,
                        CommittedAt = commitMetadata.Date, //commitMetadata.Date
                        Message = commitMetadata.Message, //commitMetadata.Message
                        Files = fileDtos //fileDtos
                    });
                    
                    Console.WriteLine("After creation of object");
                    Console.WriteLine("commitList: " + JsonSerializer.Serialize(commitList, new JsonSerializerOptions { WriteIndented = true }));
                    
                }
                Console.WriteLine("Outside");

                Console.WriteLine("Success getting commits: " + JsonSerializer.Serialize(commitList, new JsonSerializerOptions { WriteIndented = true }));
                return JsonSerializer.Serialize(commitList, new JsonSerializerOptions { WriteIndented = true });
            } catch (Exception ex)
            {
                Console.WriteLine("Error getting commits: " + ex.ToString());
                return null;
            }
            
        }



        // Auth header


        

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




    }
}
