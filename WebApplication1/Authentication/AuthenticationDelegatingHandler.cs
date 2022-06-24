using Microsoft.AspNetCore.Http.Extensions;
using System.Net;
using System.Net.Http.Headers;
using System.Web.Http;
using WebApplication1.Services;

namespace WebApplication1.Authentication
{
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthenticationDelegatingHandler(IAuthenticationService authenticationService, IHttpContextAccessor contextAccessor)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService)); ;
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessTokenCookie = _contextAccessor.HttpContext?.Request.Cookies.FirstOrDefault(i => i.Key == "access_token").Value;
            var refreshTokenCookie = _contextAccessor.HttpContext?.Request.Cookies.FirstOrDefault(i => i.Key == "refresh_token").Value;

            (TokenModel? accessToken, TokenModel? refreshToken) = 
                await _authenticationService
                      .RetrieveToken(
                        accessTokenCookie,
                        refreshTokenCookie
                        );

            if (accessToken == null && refreshToken == null)
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                if(accessToken.SetNewCookie) _contextAccessor.HttpContext?.Response.Headers
                        .Append("Set-Cookie", $"access_token={accessToken.Token};" +
                                "httponly;" +
                                "secure;" +
                                $"max-age={accessToken.ExpiresIn};");

                if(refreshToken.SetNewCookie) _contextAccessor.HttpContext?.Response.Headers
                        .Append("Set-Cookie", $"refresh_token={refreshToken.Token};" +
                                "httponly;" +
                                "secure;" +
                                $"max-age={TimeSpan.FromDays(60).TotalSeconds};");
            }

            return response;
        }
    }
}
