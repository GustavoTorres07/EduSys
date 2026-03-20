using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace EduSys.Web.Auth
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;

        // ✅ Uso de constantes para evitar "Magic Strings" propensos a errores de tipeo
        public const string AuthTokenKey = "authToken";
        public const string UserNombreKey = "UserNombre";
        public const string UserApellidoKey = "UserApellido";
        public const string UserFotoKey = "UserFoto";

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>(AuthTokenKey);

            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = ParseClaimsFromJwt(token).ToList();

            // ✅ Recuperar datos visuales del LocalStorage (Para sobrevivir al F5)
            var nombre = await _localStorage.GetItemAsync<string>(UserNombreKey);
            var apellido = await _localStorage.GetItemAsync<string>(UserApellidoKey);
            var foto = await _localStorage.GetItemAsync<string>(UserFotoKey);

            // Aseguramos no duplicar claims si el JWT ya los trajera en un futuro
            if (!string.IsNullOrEmpty(nombre) && !claims.Any(c => c.Type == "Nombre"))
                claims.Add(new Claim("Nombre", nombre));

            if (!string.IsNullOrEmpty(apellido) && !claims.Any(c => c.Type == "Apellido"))
                claims.Add(new Claim("Apellido", apellido));

            if (!string.IsNullOrEmpty(foto))
            {
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
            await _localStorage.SetItemAsync(AuthTokenKey, token);
            await _localStorage.SetItemAsync(UserNombreKey, nombre ?? "");
            await _localStorage.SetItemAsync(UserApellidoKey, apellido ?? "");
            await _localStorage.SetItemAsync(UserFotoKey, fotoUrl ?? "");

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
            await _localStorage.RemoveItemAsync(AuthTokenKey);
            await _localStorage.RemoveItemAsync(UserNombreKey);
            await _localStorage.RemoveItemAsync(UserApellidoKey);
            await _localStorage.RemoveItemAsync(UserFotoKey);

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