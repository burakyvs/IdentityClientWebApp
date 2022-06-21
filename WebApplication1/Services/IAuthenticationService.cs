namespace WebApplication1.Services
{
    public interface IAuthenticationService
    {
        Task<string> RetrieveToken();
    }
}
