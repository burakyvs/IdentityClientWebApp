using System.Net.Http.Headers;
using WebApplication1.Services;

namespace WebApplication1.Authentication
{
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationDelegatingHandler(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _authenticationService.RetrieveToken();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
