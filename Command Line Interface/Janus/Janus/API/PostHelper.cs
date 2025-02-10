using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;

namespace Janus.API
{
    public static class ApiHelper
    {


        public static async Task<(bool Success, TResponse responseData, string Message)> SendPostRequestAsync<TRequest, TResponse>(string url, string pat, PatAuth.AuthBody requestData)
        {
            
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                UseProxy = false
            };
            
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.ExpectContinue = false;
                client.DefaultRequestVersion = HttpVersion.Version11;
                client.Timeout = TimeSpan.FromSeconds(30);

                Console.WriteLine("Actual send");

                Console.WriteLine("URL: " + url);
                Console.WriteLine("PAT: " + pat);
                // print the data to console
                Console.WriteLine("DATA: " + JsonSerializer.Serialize(requestData));


                // Set bearer token
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", pat);

                // Serialised data to json
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");


                HttpResponseMessage response;
                try
                {
                    foreach (var header in client.DefaultRequestHeaders)
                    {
                        Console.WriteLine($"Headers: {header.Key}: {string.Join(",", header.Value)}");
                    }


                    Console.WriteLine("Actual sending");
                    // Send request
                    response = await client.PostAsync(url, content);

                    Console.WriteLine("After sending");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error actual sending");

                    // Handle errors with post (e.g. network issues)
                    return (false, default, $"Error sending request: {ex.Message}");
                }

                Console.WriteLine("Actual receive");

                // Check for success
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return (false, default, $"Error response: {errorMessage}");
                }

                TResponse data;
                try
                {
                    data = await response.Content.ReadFromJsonAsync<TResponse>();
                    Console.WriteLine("valid: " + data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading json");
                    return (false, default, $"Error with response: {ex.Message}");
                }

                return (true, data, null);
            }

        }


    }
}
