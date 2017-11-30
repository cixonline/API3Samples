using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DesktopClient {

    class Program {

        static void Main(string[] args) {

            Console.WriteLine(GetResponse().Result);
        }

        public static async Task<string> GetResponse() {

            HttpClient _client = new HttpClient(new HttpClientHandler());

            // Call the OpenID configuration source
            HttpResponseMessage response = await _client.GetAsync("https://api.cix.uk/.well-known/openid-configuration", 
                default(CancellationToken)).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) {
                return response.ReasonPhrase;
            }

            // Read the discovery data
            string disco = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            string tokenEndPoint = JObject.Parse(disco)["token_endpoint"].ToString();

            string username = "your_cix_username";
            string password = "your_cix_password";

            string yourClientId = "cixDesktop";
            string yourClientSecret = "secret";

            // To get the access token, we need to pass through the CIX username and password as the
            // form data, specifying grant type as 'password' for Resource Owner Password Grant. The
            // scope is also required to specify the part of the API to which we're requesting access.
            var fields = new Dictionary<string, string> {
                { "grant_type", "password" },
                { "username", username },
                { "password", password },
                { "scope", "cixApi3" }
            };

            // Pass through the client Id and secret as the authentication scheme.
            Encoding encoding = Encoding.UTF8;
            string credential = String.Format("{0}:{1}", yourClientId, yourClientSecret);

            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndPoint) {
                Content = new FormUrlEncodedContent(fields),
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("basic", Convert.ToBase64String(encoding.GetBytes(credential)));

            // Now get the access token required for making the API call
            string accessToken;
            try {
                response = await _client.SendAsync(request, default(CancellationToken)).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode) {
                    return response.ReasonPhrase;
                }
                if (response.Content == null) {
                    return "Empty content";
                }
                string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                accessToken = JObject.Parse(content)["access_token"].ToString();
            }
            catch (Exception ex) {
                return ex.Message;
            }

            // Now that we have an access token, we can use it to make the API call by specifying the
            // access token in the Bearer header field.
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Call the API
            response =  await client.GetAsync("https://api.cix.uk/v3.0/Forum/cixnews/details");
            if (!response.IsSuccessStatusCode) {
                return response.StatusCode.ToString();
            }

            // Return the response
            return await response.Content.ReadAsStringAsync();
        }
    }
}
