using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Domain.IdentityStores.Settings
{
    public class TokenInfo
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public Guid RefreshToken { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("user_id")]
        public string UserId { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("role")]
        public string Role { get; set; }
        [JsonProperty("completed_register")]
        public bool CompletedRegister { get; set; }
        [JsonProperty("invitation_code")]
        public string InvitationCode { get; set; }
        [JsonProperty("team_id")]
        public string TeamId { get; set; }
        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }
        [JsonProperty("have_dependents")]
        public bool Dependents { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("adminAccesses")]
        public List<string> AdminAccesses { get; set; }
        [JsonProperty("first_access")]
        public bool FirstAccess { get; set; }
        [JsonProperty("email_verified")]
        public bool EmailVerified { get; set; }
    }
}