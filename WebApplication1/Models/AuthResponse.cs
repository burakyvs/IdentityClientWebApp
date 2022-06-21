using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    public class AuthResponse
    {
        [JsonPropertyName("access_token")]
        public string Token { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("expires_in")]
        public int Expiration { get; set; }
    }
}
