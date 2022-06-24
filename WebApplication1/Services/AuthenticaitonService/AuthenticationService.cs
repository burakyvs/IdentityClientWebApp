using IdentityModel.Client;
using WebApplication1.Authentication;

namespace WebApplication1.Services
{
    public class AuthenticationService : HttpService, IAuthenticationService
    {
        private readonly HttpClient _httpClient;

        public override HttpClient HttpClient { get => _httpClient; init => _httpClient = value; }

        public AuthenticationService(HttpClient httpclient)
        {
            _httpClient = httpclient;
        }

        public async Task<(TokenModel?, TokenModel?)> RetrieveNewToken()
        {
            PasswordTokenRequest tokenRequest = new PasswordTokenRequest
            {
                Address = AuthenticationServiceConsts.GetTokenAddress,
                ClientId = AuthenticationServiceConsts.ClientId,
                ClientSecret = AuthenticationServiceConsts.ClientSecret,
                Scope = "profile offline_access catalog.api.read catalog.api.write",
                UserName = "test3",
                Password = "Test123."
            };

            // get tokens using password grant type.
            var response = await HttpClient.RequestPasswordTokenAsync(tokenRequest).ConfigureAwait(false);

            if (response.IsError)
                return (null, null);


            var accessToken = response.AccessToken ?? throw new ArgumentNullException(nameof(response.AccessToken));
            var refreshToken = response.RefreshToken ?? throw new ArgumentNullException(nameof(response.RefreshToken));

            TokenModel accessTokenModel = new TokenModel()
            {
                Token = accessToken,
                ExpiresIn = response.ExpiresIn,
                SetNewCookie = true
            };

            TokenModel refreshTokenModel = new TokenModel()
            {
                Token = refreshToken,
                ExpiresIn = (int)TimeSpan.FromDays(60).TotalSeconds,
                SetNewCookie = true
            };

            return (accessTokenModel, refreshTokenModel);
        }

        public async Task<(TokenModel?, TokenModel?)> RetrieveToken(string? accessToken = null, string? refreshToken = null)
        {
            if (accessToken == null && refreshToken != null)
            {
                // GET TOKEN VIA REFRESH TOKEN

                return await RetrieveTokenViaRefreshTokenAsync(refreshToken);
            }
            else if (accessToken != null)
            {
                // CHECK EXPIRATION
                bool isTokenExpired = await IsTokenExpired(accessToken);

                if (!isTokenExpired)
                    return (new TokenModel { Token = accessToken }, new TokenModel());

                if (refreshToken != null)
                {
                    // GET TOKEN VIA REFRESH TOKEN

                    return await RetrieveTokenViaRefreshTokenAsync(refreshToken);
                }
            }

            // REDIRECT TO LOGIN
            return (null, null);
        }

        private async Task<(TokenModel, TokenModel)> RetrieveTokenViaRefreshTokenAsync(string refreshToken)
        {

            RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest
            {
                Address = AuthenticationServiceConsts.GetTokenAddress,
                ClientId = AuthenticationServiceConsts.ClientId,
                ClientSecret = AuthenticationServiceConsts.ClientSecret,
                RefreshToken = refreshToken
            };

            // get a new access_token via refresh_token.
            var refreshTokenResponse = await HttpClient.RequestRefreshTokenAsync(refreshTokenRequest).ConfigureAwait(false);
            var accessToken = refreshTokenResponse.AccessToken ?? throw new ArgumentNullException(nameof(refreshTokenResponse.AccessToken));

            TokenModel accessTokenModel = new TokenModel()
            {
                Token = accessToken,
                ExpiresIn = refreshTokenResponse.ExpiresIn,
                SetNewCookie = true
            };

            TokenModel refreshTokenModel = new TokenModel()
            {
                Token = refreshToken
            };

            return (accessTokenModel, refreshTokenModel);
        }

        private async Task<bool> IsTokenExpired(string accessToken)
        {
            var queryParameters = new Dictionary<string, string>
            {
                { "accessToken", accessToken }
            };
            var dictFormUrlEncoded = new FormUrlEncodedContent(queryParameters);
            var queryString = await dictFormUrlEncoded.ReadAsStringAsync();

            var accessTokenExpirationResponse = await HttpClient.GetAsync($"{AuthenticationServiceConsts.CheckTokenExpirationAddress}?{queryString}");

            return !accessTokenExpirationResponse.IsSuccessStatusCode;
        }
    }
}
