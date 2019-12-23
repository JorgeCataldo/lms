using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ExternalService
{
    public class LinkedIn
    {
        public static async Task<HttpResponseMessage> AccessToken(
            string code, IConfiguration configuration
        ) {
            HttpClient client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", configuration[$"LinkedIn:linkedInRedirect"]),
                new KeyValuePair<string, string>("client_id", configuration[$"LinkedIn:linkedInId"]),
                new KeyValuePair<string, string>("client_secret", configuration[$"LinkedIn:linkedInSecret"])
            });
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return await client.PostAsync("https://www.linkedin.com/oauth/v2/accessToken", content);
        }

        public static async Task<HttpResponseMessage> DadosBasicos(
            string token
        )
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await client.GetAsync("https://api.linkedin.com/v2/me?projection=(id,firstName,lastName,profilePicture(displayImage~:playableStreams))");
        }

        public static async Task<HttpResponseMessage> Email(
            string token
        )
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await client.GetAsync("https://api.linkedin.com/v2/emailAddress?q=members&projection=(elements*(handle~))");
        }
    }
}
