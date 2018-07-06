using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace app.Services
{
    public interface IApiService
    {
        Task<IList<string>> GetValues(string token = null);
    }

    public class SimpleApiService : IApiService
    {
        private HttpClient client = new HttpClient();
        private readonly ITokenService tokenService;
        public SimpleApiService(ITokenService tokenService)
        {
            this.tokenService = tokenService;
        }

        public async Task<IList<string>> GetValues(string inputToken = null)
        {
            List<string> values = new List<string>();
            var token = inputToken ?? (await tokenService.GetTokenAsync());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await client.GetAsync("http://localhost:51482/api/values");
            var json = await res.Content.ReadAsStringAsync();
            if (res.IsSuccessStatusCode)
            {                
                values = JsonConvert.DeserializeObject<List<string>>(json);
            }
            else
            {
                values = new List<string> { res.StatusCode.ToString(), res.ReasonPhrase, json };
            }
            return values;
        }
    }
}
