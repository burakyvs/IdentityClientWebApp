using WebApplication1.Authentication;

namespace WebApplication1.Services
{
    public interface IAuthenticationService
    {
        Task<(TokenModel, TokenModel)> RetrieveToken(string? accessToken = null, string? refreshToken = null);
    }
}
