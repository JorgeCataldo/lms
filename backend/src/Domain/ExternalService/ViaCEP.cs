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
    public class ViaCEP
    {
        public static async Task<HttpResponseMessage> CEP(
            string cep
        )
        {
            HttpClient client = new HttpClient();
            return await client.GetAsync("https://viacep.com.br/ws/" + cep + "/json/");
        }
    }
}