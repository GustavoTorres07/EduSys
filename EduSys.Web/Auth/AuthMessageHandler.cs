using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace EduSys.Web.Auth
{
    /// <summary>
    /// Intercepta todas las peticiones HTTP salientes y adjunta el Token JWT si existe.
    /// </summary>
    public class AuthMessageHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;

        public AuthMessageHandler(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Leemos el token del LocalStorage
            var token = await _localStorage.GetItemAsync<string>(CustomAuthStateProvider.AuthTokenKey);

            if (!string.IsNullOrWhiteSpace(token))
            {
                // Si hay token, lo inyectamos en la cabecera Authorization
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Continuamos con el flujo normal de la petición HTTP
            return await base.SendAsync(request, cancellationToken);
        }
    }
}