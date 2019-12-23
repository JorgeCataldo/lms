using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ECommerceIntegration.Woocommerce
{
    public class Woocommerce
    {
        private static HttpClient http = new HttpClient();

        public static async Task<HttpResponseMessage> CreateProduct(
            EcommerceItem product, IConfiguration configuration
        ) {
            string createProductUrl = configuration[$"EcommerceIntegration:ApiPath"] +
                configuration[$"EcommerceIntegration:Paths:CreateProductPath"] +
                "?consumer_key=" + configuration[$"EcommerceIntegration:Secrets:ConsumerKey"] +
                "&consumer_secret=" + configuration[$"EcommerceIntegration:Secrets:ConsumerSecret"];

            return await http.PostAsync(
                createProductUrl,
                new StringContent(
                    JsonConvert.SerializeObject(product),
                    Encoding.UTF8,
                    "application/json"
                )
            );
        }

        public static async Task<HttpResponseMessage> UpdateProduct(
            EcommerceItem product, IConfiguration configuration
        ) {
            string createProductUrl = configuration[$"EcommerceIntegration:ApiPath"] +
                configuration[$"EcommerceIntegration:Paths:UpdateProductPath"] + product.product.id +
                "?consumer_key=" + configuration[$"EcommerceIntegration:Secrets:ConsumerKey"] +
                "&consumer_secret=" + configuration[$"EcommerceIntegration:Secrets:ConsumerSecret"];

            return await http.PutAsync(
                createProductUrl,
                new StringContent(
                    JsonConvert.SerializeObject(product),
                    Encoding.UTF8,
                    "application/json"
                )
            );
        }

        public static async Task<HttpResponseMessage> GetOrderById(
            long orderId, IConfiguration configuration
        ) {
            string getOrderUrl = configuration[$"EcommerceIntegration:ApiPath"] +
                configuration[$"EcommerceIntegration:Paths:GetOrderPath"] + orderId +
                "?consumer_key=" + configuration[$"EcommerceIntegration:Secrets:ConsumerKey"] +
                "&consumer_secret=" + configuration[$"EcommerceIntegration:Secrets:ConsumerSecret"];

            return await http.GetAsync(
                getOrderUrl
            );
        }
    }
}
