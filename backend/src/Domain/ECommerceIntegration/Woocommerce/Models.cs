namespace Domain.ECommerceIntegration.Woocommerce
{
    public class EcommerceItem
    {
        public ProductItem product { get; set; }
    }

    public class ProductItem
    {
        public long? id { get; set; }
        public bool visible { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string regular_price { get; set; }
        public string description { get; set; }
    }
}
