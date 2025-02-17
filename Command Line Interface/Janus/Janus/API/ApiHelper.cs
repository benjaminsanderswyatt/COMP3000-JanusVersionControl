using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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



        public static async Task<bool> DownloadBatchFilesAsync(List<string> fileHashes, string destinationFolder, string pat)
        {
            string apiUrl = "https://localhost:82/api/cli/repo/batchfiles";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", pat);
                

                // Send request with list of file hashes
                var requestBody = new StringContent(JsonSerializer.Serialize(fileHashes), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch files: {await response.Content.ReadAsStringAsync()}");
                    return false;
                }

                // Stream the response
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(responseStream))
                {
                    string? currentFileName = null;
                    FileStream? fileStream = null;

                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();

                        if (line.StartsWith("FILE:")) // Start of a new file
                        {
                            if (fileStream != null) 
                                await fileStream.DisposeAsync();

                            currentFileName = line.Split(':')[1];
                            string filePath = Path.Combine(destinationFolder, currentFileName);
                            fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                        }
                        else if (line == "---") // Separator between files
                        {
                            if (fileStream != null) 
                                await fileStream.DisposeAsync();
                            fileStream = null;
                        }
                        else if (fileStream != null) // Write content to the file
                        {
                            byte[] bytes = Encoding.UTF8.GetBytes(line + "\n");
                            await fileStream.WriteAsync(bytes, 0, bytes.Length);
                        }
                    }

                    if (fileStream != null) 
                        await fileStream.DisposeAsync();
                }
            }

            return true;
        }








    }
}
        
