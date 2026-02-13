using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace EduSys.Web.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _httpClient;

        public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
        {
            _localStorage = localStorage;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // 1. Obtener Token
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 2. Parsear Claims básicos del Token
            var claims = ParseClaimsFromJwt(token).ToList();

            // 3. ✅ RECUPERAR DATOS DEL LOCALSTORAGE (ESTA ES LA CLAVE)
            // Si no recuperamos esto aquí, al dar F5 se pierden los datos visuales
            var nombre = await _localStorage.GetItemAsync<string>("UserNombre");
            var apellido = await _localStorage.GetItemAsync<string>("UserApellido");
            var foto = await _localStorage.GetItemAsync<string>("UserFoto"); // <--- IMPORTANTE

            if (!string.IsNullOrEmpty(nombre)) claims.Add(new Claim("Nombre", nombre));
            if (!string.IsNullOrEmpty(apellido)) claims.Add(new Claim("Apellido", apellido));

            // ✅ Aseguramos que el Claim exista siempre si hay foto guardada
            if (!string.IsNullOrEmpty(foto))
            {
                // Removemos si ya existía para evitar duplicados y ponemos el del storage
                var existing = claims.FirstOrDefault(c => c.Type == "FotoPerfilUrl");
                if (existing != null) claims.Remove(existing);

                claims.Add(new Claim("FotoPerfilUrl", foto));
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }

        public async Task MarcarUsuarioComoAutenticado(string token, string nombre, string apellido, string fotoUrl)
        {
            await _localStorage.SetItemAsync("authToken", token);
            await _localStorage.SetItemAsync("UserNombre", nombre ?? "");
            await _localStorage.SetItemAsync("UserApellido", apellido ?? "");

            // ✅ Guardamos la foto (incluso si es null guardamos cadena vacía)
            await _localStorage.SetItemAsync("UserFoto", fotoUrl ?? "");

            var claims = ParseClaimsFromJwt(token).ToList();

            if (!string.IsNullOrEmpty(nombre)) claims.Add(new Claim("Nombre", nombre));
            if (!string.IsNullOrEmpty(apellido)) claims.Add(new Claim("Apellido", apellido));
            if (!string.IsNullOrEmpty(fotoUrl)) claims.Add(new Claim("FotoPerfilUrl", fotoUrl));

            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

            NotifyAuthenticationStateChanged(authState);
        }

        public async Task MarcarUsuarioComoDeslogueado()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("UserNombre");
            await _localStorage.RemoveItemAsync("UserApellido");
            await _localStorage.RemoveItemAsync("UserFoto");

            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));

            NotifyAuthenticationStateChanged(authState);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs == null) return claims;

            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Key == "role" || kvp.Key == ClaimTypes.Role)
                {
                    if (kvp.Value.ToString()!.Trim().StartsWith("["))
                    {
                        var parsedRoles = JsonSerializer.Deserialize<string[]>(kvp.Value.ToString()!);
                        if (parsedRoles != null)
                            foreach (var parsedRole in parsedRoles)
                                claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, kvp.Value.ToString()!));
                    }
                }
                else if (kvp.Key == "nameid" || kvp.Key == "sub")
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, kvp.Value.ToString()!));
                }
                else
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value.ToString()!));
                }
            }
            return claims;
        }

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}