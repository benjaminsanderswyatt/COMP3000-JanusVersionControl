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

                if (responseEndpoint.IsSuccessStatusCode)
                {
                    string stringResponseData = await responseEndpoint.Content.ReadAsStringAsync();

                    return (true, stringResponseData);
                }
                else
                {
                    string reason = await responseEndpoint.Content.ReadAsStringAsync();

                    return (false, reason);
                }

            }
        }

        /*
        public static async Task SendAuthenticateAsync()
        {
            Console.WriteLine("Starting test POST request...");

            string url = "https://localhost:82/api/AccessToken/Authenticate";
            string pat = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEiLCJUb2tlblR5cGUiOiJQQVQiLCJleHAiOjE3MzkzMDgwMzQsImlzcyI6IkNMSUlzc3VlciIsImF1ZCI6IkNMSUF1ZGllbmNlIn0.P9hxDLL2oRGBCHkiqF7NxOGwbgL7EMIpygov8jMPeIQ";
            var requestData = new { Email = "test@test.com" };

            using (var client = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true }))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", pat);

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync(url, content);
                    string responseString = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"Response Status: {response.StatusCode}");
                    Console.WriteLine($"Response Body: {responseString}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

        }
        
        */
    }
        

    

}
        
