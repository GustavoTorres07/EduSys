using Blazored.LocalStorage;
using EduSys.Shared.DTOs;
using EduSys.Web.Auth;
using EduSys.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(HttpClient httpClient,
                           ILocalStorageService localStorage,
                           AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<SesionDTO> Login(LoginDTO loginDTO)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDTO);

            if (!response.IsSuccessStatusCode) return null;

            // Leer respuesta
            var jsonString = await response.Content.ReadAsStringAsync();
            var sesion = System.Text.Json.JsonSerializer.Deserialize<SesionDTO>(jsonString,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (sesion != null)
            {
                // ✅ CORRECCIÓN AQUÍ: Agregamos sesion.FotoPerfilUrl
                await ((CustomAuthStateProvider)_authStateProvider).MarcarUsuarioComoAutenticado(
                    sesion.Token,
                    sesion.Nombre,
                    sesion.Apellido,
                    sesion.FotoPerfilUrl // <--- Faltaba este 4to argumento
                );
            }

            return sesion;
        }

        public async Task Logout()
        {
            await ((CustomAuthStateProvider)_authStateProvider).MarcarUsuarioComoDeslogueado();
        }

        public async Task<bool> CambiarClaveAsync(string nuevaClave)
        {
            var dto = new CambioClaveDTO { NuevaClave = nuevaClave };
            var response = await _httpClient.PostAsJsonAsync("api/auth/cambiar-clave", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RecuperarClaveAsync(string email)
        {
            var dto = new RecuperarClaveRequestDTO { Email = email };
            var response = await _httpClient.PostAsJsonAsync("api/auth/recuperar", dto);
            return response.IsSuccessStatusCode;
        }
    }
}