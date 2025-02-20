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
            string apiUrl = $"https://localhost:82/api/cli/repo/batchfiles/{owner}/{repoName}";

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

                // Stream the response
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    // Get bytes of seperators
                    byte[] separator = Encoding.UTF8.GetBytes("\n---\n");
                    byte[] newline = { (byte)'\n' };

                    while (true)
                    {
                        // Read metadata line
                        List<byte> metadataBytes = new List<byte>();
                        byte[] buffer = new byte[1];
                        bool newlineFound = false;

                        while (await responseStream.ReadAsync(buffer, 0, 1) > 0)
                        {
                            if (buffer[0] == newline[0]) // Reaches end of line
                            {
                                newlineFound = true;
                                break;
                            }
                            metadataBytes.Add(buffer[0]);
                        }

                        // Check if end of stream
                        if (!newlineFound && metadataBytes.Count == 0)
                            break;

                        // Get metadata of file
                        string metadataLine = Encoding.UTF8.GetString(metadataBytes.ToArray());
                        string[] metadataParts = metadataLine.Split('|');

                        if (metadataParts.Length != 3)
                            throw new Exception("Invalid metadata");

                        string fileType = metadataParts[0];
                        string fileHash = metadataParts[1];
                        long fileLength = long.Parse(metadataParts[2]);

                        // Read file content
                        string filePath = Path.Combine(destinationFolder, fileHash);

                        await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            byte[] contentBuffer = new byte[2048]; // 2048 is buffer size (balance memory, disk I/O, with file size in mind)
                            long bytesRemaining = fileLength;

                            // Write the bytes
                            while (bytesRemaining > 0)
                            {
                                int bytesToRead = (int)Math.Min(contentBuffer.Length, bytesRemaining);
                                int bytesRead = await responseStream.ReadAsync(contentBuffer, 0, bytesToRead);

                                if (bytesRead == 0)
                                    throw new Exception("Unexpected end of stream");

                                await fileStream.WriteAsync(contentBuffer, 0, bytesRead);
                                bytesRemaining -= bytesRead;
                            }

                        }

                        // Read seperator
                        byte[] actualSeparator = new byte[separator.Length];
                        int bytesReadSeparator = await responseStream.ReadAsync(actualSeparator, 0, separator.Length);

                        // Check seperator is valid
                        if (bytesReadSeparator != separator.Length || !actualSeparator.SequenceEqual(separator))
                            throw new Exception("Invalid separator");

                    }
                }
            }

            return true;
        }







    }
}

