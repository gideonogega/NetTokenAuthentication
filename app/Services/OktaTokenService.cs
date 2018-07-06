using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using app.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace app.Services
{
    public interface ITokenService
    {
        Task<string> GetTokenAsync();
    }

    public class OktaTokenService : ITokenService
    {
        private OktaToken token = new OktaToken();
        private readonly IOptions<OktaSettings> oktaSettings;

        public OktaTokenService(IOptions<OktaSettings> oktaSettings)
        {
            this.oktaSettings = oktaSettings;
        }

        public async Task<string> GetTokenAsync()
        {
            if (!this.token.IsValidAndNotExpiring)
            {
                this.token = await this.GetNewAccessToken();
            }
            return token.AccessToken;
        }

        private async Task<OktaToken> GetNewAccessToken()
        {
            var client = new HttpClient();
            var client_id = this.oktaSettings.Value.ClientId;
            var client_secret = this.oktaSettings.Value.ClientSecret;
            var clientCreds = System.Text.Encoding.UTF8.GetBytes($"{client_id}:{client_secret}");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", System.Convert.ToBase64String(clientCreds));

            var postMessage = new Dictionary<string, string>();
            postMessage.Add("grant_type", "client_credentials");
            postMessage.Add("scope", "access_token");
            var request = new HttpRequestMessage(HttpMethod.Post, this.oktaSettings.Value.TokenUrl)
            {
                Content = new FormUrlEncodedContent(postMessage)
            };

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                token = JsonConvert.DeserializeObject<OktaToken>(json);
                token.ExpiresAt = DateTime.UtcNow.AddSeconds(this.token.ExpiresIn);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new ApplicationException("Unable to retrieve access token from Okta:" 
                    + Environment.NewLine + content);
            }
            return token;
        }
    }
}
