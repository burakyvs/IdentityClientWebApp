using AutoMapper;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;

        public AuthenticationService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, IMapper mapper)
        {
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
            _mapper = mapper;
        }

        public async Task<string> RetrieveToken()
        {
            string accessToken;
            string refreshToken;

            if (!_memoryCache.TryGetValue("AccessToken", out accessToken))
            {

                // check for refresh_token availability
                if (_memoryCache.TryGetValue("RefreshToken", out refreshToken))
                {
                    accessToken = await RetrieveTokenViaRefreshTokenAsync(refreshToken);

                    return accessToken;
                }

                // redirect to login
                #region Login

                var httpClient = _httpClientFactory.CreateClient("IdentityClient");
                //var tokenClient = new TokenClient(httpClient, new TokenClientOptions() { );

                PasswordTokenRequest tokenRequest = new PasswordTokenRequest
                {
                    Address = AuthenticationServiceConsts.Address,
                    ClientId = AuthenticationServiceConsts.ClientId,
                    ClientSecret = AuthenticationServiceConsts.ClientSecret,
                    Scope = "profile offline_access catalog.api.read catalog.api.write",
                    UserName = "test3",
                    Password = "Test123."
                };

                var response = await httpClient.RequestPasswordTokenAsync(tokenRequest).ConfigureAwait(false);
                accessToken = response.AccessToken ?? throw new ArgumentNullException(nameof(response.AccessToken));
                refreshToken = response.RefreshToken ?? throw new ArgumentNullException(nameof(response.RefreshToken));

                TimeSpan accessTokenExpiresIn = TimeSpan.FromSeconds(response.ExpiresIn);
                TimeSpan refreshTokenExpiresIn = TimeSpan.FromDays(60);

                _memoryCache.Set("AccessToken", accessToken, accessTokenExpiresIn);
                _memoryCache.Set("RefreshToken", refreshToken, refreshTokenExpiresIn);

                #endregion
            }
            return accessToken;
        }

        private async Task<string> RetrieveTokenViaRefreshTokenAsync(string refreshToken)
        {
            var httpClient = _httpClientFactory.CreateClient("IdentityClient");

            RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest
            {
                Address = AuthenticationServiceConsts.Address,
                ClientId = AuthenticationServiceConsts.ClientId,
                ClientSecret = AuthenticationServiceConsts.ClientSecret,
                RefreshToken = refreshToken
            };

            var refreshTokenResponse = await httpClient.RequestRefreshTokenAsync(refreshTokenRequest).ConfigureAwait(false);
            var accessToken = refreshTokenResponse.AccessToken ?? throw new ArgumentNullException(nameof(response.AccessToken));

            TimeSpan rtAccessTokenExpiresIn = TimeSpan.FromSeconds(refreshTokenResponse.ExpiresIn);

            _memoryCache.Set("AccessToken", accessToken, rtAccessTokenExpiresIn);

            return accessToken;
        }
    }
}
