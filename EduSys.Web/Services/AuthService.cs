using EduSys.Shared.DTOs;
using EduSys.Web.Auth;
using EduSys.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILogger<AuthService> _logger; // ✅ Agregado para depuración

        // 💡 ILocalStorageService eliminado porque no se usaba en esta clase
        public AuthService(
            HttpClient httpClient,
            AuthenticationStateProvider authStateProvider,
            ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
            _logger = logger;
        }

        public async Task<SesionDTO?> LoginAsync(LoginDTO loginDTO)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDTO);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Intento de login fallido para {Email}: {ErrorMsg}", loginDTO.Email, errorMsg);
                    return null;
                }

                // ✅ Lectura JSON optimizada (Nativa de System.Net.Http.Json)
                var sesion = await response.Content.ReadFromJsonAsync<SesionDTO>();

                if (sesion != null)
                {
                    // Actualizamos el estado global de la app
                    await ((CustomAuthStateProvider)_authStateProvider).MarcarUsuarioComoAutenticado(
                        sesion.Token,
                        sesion.Nombre,
                        sesion.Apellido,
                        sesion.FotoPerfilUrl
                    );
                }

                return sesion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico de conexión al intentar iniciar sesión.");
                return null;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await ((CustomAuthStateProvider)_authStateProvider).MarcarUsuarioComoDeslogueado();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar cerrar la sesión localmente.");
            }
        }

        public async Task<bool> CambiarClaveAsync(string nuevaClave)
        {
            try
            {
                var dto = new CambioClaveDTO { NuevaClave = nuevaClave };
                var response = await _httpClient.PostAsJsonAsync("api/auth/cambiar-clave", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al cambiar clave: {ErrorMsg}", errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexión al intentar cambiar la clave.");
                return false;
            }
        }

        public async Task<bool> RecuperarClaveAsync(string email)
        {
            try
            {
                var dto = new RecuperarClaveRequestDTO { Email = email };
                var response = await _httpClient.PostAsJsonAsync("api/auth/recuperar", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al solicitar recuperación de clave para {Email}: {ErrorMsg}", email, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexión al intentar recuperar la clave para {Email}.", email);
                return false;
            }
        }
    }
}