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
            string apiUrl = "https://localhost:82/api/cli/" + endpoint;

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



       
    }
        

    

}
        
