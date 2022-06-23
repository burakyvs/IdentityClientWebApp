namespace WebApplication1.Authentication
{
    public class TokenModel
    {
        public string Token { get; set; } = string.Empty;
        public int ExpiresIn { get; set; } = 3600;
        public bool SetNewCookie { get; set; } = false;
    }
}
