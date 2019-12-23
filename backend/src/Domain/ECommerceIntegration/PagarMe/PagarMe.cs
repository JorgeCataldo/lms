using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ECommerceIntegration.PagarMe
{
    public class PagarMe
    {
        private static HttpClient http = new HttpClient();

        public static async Task<HttpResponseMessage> GetTransactions(IConfiguration configuration)
        {
            try
            {
                string getTransactionUrl = configuration[$"PagarMeIntegration:ApiPath"] +
                    configuration[$"PagarMeIntegration:Paths:GetTransactionsPath"] +
                    "?api_key=" + configuration[$"PagarMeIntegration:Secrets:ApiKey"] + "&count=2500";
                //+ "&date_updated=>=1543111200&date_updated=<1571626800"

                return await http.GetAsync(
                    getTransactionUrl
                );
            }
            catch(Exception ex)
            {
                string content = JsonConvert.SerializeObject(ex);

                var exc = new Exception($"get_transaction_error: {content}");
                SentrySdk.CaptureException(exc);
            }

            return null;
        }

        public static async Task<HttpResponseMessage> GetTransactionsPayables(string transactionId, IConfiguration configuration)
        {
            string getTransactionPayablesUrl = configuration[$"PagarMeIntegration:ApiPath"] +
                configuration[$"PagarMeIntegration:Paths:GetTransactionsPath"] + transactionId + 
                configuration[$"PagarMeIntegration:Paths:GetPayblesPath"] +
                "?api_key=" + configuration[$"PagarMeIntegration:Secrets:ApiKey"];

            return await http.GetAsync(
                getTransactionPayablesUrl
            );
        }

    }
}
