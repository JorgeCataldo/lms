namespace Domain.IdentityStores.Settings
{
    public class JwtOptions
    {
        public string JwtIssuer { get; set; }
        public string JwtAudience { get; set; }
        public string JwtSecret { get; set; }
        public int JwtExpirationMinutes { get; set; }        
    }
}