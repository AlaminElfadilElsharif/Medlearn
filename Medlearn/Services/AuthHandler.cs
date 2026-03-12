// Handlers/AuthHandler.cs
using Medlearn.Services;
using Medlearn.Services.Implementations;

namespace Medlearn.Handlers
{
    public class AuthHandler : DelegatingHandler
    {
        private readonly ITokenService _tokenService;

        public AuthHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Get token from TokenService
            var token = await _tokenService.GetTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            // Handle 401 responses
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("Unauthorized request - token may be invalid or expired");
            }

            return response;
        }
    }
}
