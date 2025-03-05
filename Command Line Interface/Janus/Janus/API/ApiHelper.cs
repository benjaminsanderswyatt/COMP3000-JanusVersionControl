using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Janus.API
{
    public static class ApiHelper
    {

        public static async Task<(bool, string)> SendPostAsync(string endpoint, object bodyObject, string? pat = null)
        {
            string apiUrl = "https://localhost:82/api/" + endpoint;

            using (HttpClient client = new HttpClient())
            {
                // Add Auth Token to header if required
                if (pat != null)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", pat);
                }


                //Convert body object to a json string
                var content = new StringContent(JsonSerializer.Serialize(bodyObject), Encoding.UTF8, "application/json");

                //Make request
                var responseEndpoint = await client.PostAsync(apiUrl, content);

                string responseData = await responseEndpoint.Content.ReadAsStringAsync();

                return responseEndpoint.IsSuccessStatusCode ? (true, responseData) : (false, responseData);
            }
        }


        public static async Task<(bool, string)> SendGetAsync(string endpoint, string? pat = null)
        {
            string apiUrl = "https://localhost:82/api/cli/repo/" + endpoint;

            using (HttpClient client = new HttpClient())
            {
                // Add Auth Token to header if required
                if (pat != null)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", pat);
                }

                var response = await client.GetAsync(apiUrl);

                string responseData = await response.Content.ReadAsStringAsync();

                return response.IsSuccessStatusCode ? (true, responseData) : (false, responseData);
            }
        }



        public static async Task<bool> DownloadBatchFilesAsync(string owner, string repoName, List<string> fileHashes, string destinationFolder, string pat)
        {
            string apiUrl = $"https://localhost:82/api/cli/repo/batchfilestest/{owner}/{repoName}";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", pat);

                // Send request with list of file hashes
                var requestBody = new StringContent(JsonSerializer.Serialize(fileHashes), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to fetch files");
                }

                // Retrieve missing files from header (if any)
                if (response.Headers.Contains("X-Missing-Files"))
                {
                    var missingFilesJson = response.Headers.GetValues("X-Missing-Files").FirstOrDefault();
                    Console.WriteLine("Missing files: " + missingFilesJson);
                }

                // Get boundary from the header
                var contentType = response.Content.Headers.ContentType;
                var boundaryParameter = contentType.Parameters.First(p => p.Name.Equals("boundary", StringComparison.OrdinalIgnoreCase));
                var boundary = HeaderUtilities.RemoveQuotes(boundaryParameter.Value).Value;

                // Read the response stream.
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    var reader = new MultipartReader(boundary, stream);
                    MultipartSection section;

                    while ((section = await reader.ReadNextSectionAsync()) != null)
                    {
                        // Get the custom header for file hash (set as X-File-Hash)
                        string fileHash = null;

                        if (section.Headers.TryGetValue("X-File-Hash", out var hashValues))
                        {
                            fileHash = hashValues.First();
                        }
                        else
                        {
                            Console.WriteLine("No file hash header found in one of the parts.");
                            continue;
                        }

                        // Find the content type of the file
                        var partContentType = section.ContentType;

                        string filePath = Path.Combine(destinationFolder, fileHash);
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                        // Save the file content
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            await section.Body.CopyToAsync(fileStream);
                        }
                    }
                }
            }

            return true;
        }






    }
}

