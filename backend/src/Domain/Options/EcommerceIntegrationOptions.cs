namespace Domain.Options
{
    public class EcommerceIntegrationOptions
    {
        public bool Active { get; }
        public string Type { get; }
        public string ApiPath { get; }
        public SecretsItem Secrets { get; }
        public PathsItem Paths { get; set; }
    }

    public class SecretsItem
    {
        public string ConsumerKey { get; }
        public string ConsumerSecret { get; }
    }

    public class PathsItem
    {
        public string CreateProductPath { get; }
        public string GetOrderPath { get; }
    }
}
