using System.Net.Http.Json;
using EduSys.Shared.DTOs;
using EduSys.Shared.Models;
using EduSys.Web.Services.Interfaces;

namespace EduSys.Web.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly HttpClient _httpClient;

        public UsuarioService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: api/usuarios
        public async Task<List<UsuarioDTO>> GetAllAsync()
        {
            try
            {
                var usuarios = await _httpClient.GetFromJsonAsync<List<UsuarioDTO>>("api/usuarios");
                return usuarios ?? new List<UsuarioDTO>();
            }
            catch (Exception)
            {
                // En caso de error (ej. 403 Forbidden o sin conexión), devolvemos lista vacía
                return new List<UsuarioDTO>();
            }
        }

        // GET: api/usuarios/{id}
        public async Task<UsuarioDTO?> GetByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UsuarioDTO>($"api/usuarios/{id}");
            }
            catch (Exception)
            {
                return null;
            }
        }

        // POST: api/usuarios
        public async Task<Usuario?> CrearAsync(Usuario usuario)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/usuarios", usuario);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Usuario>();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // PUT: api/usuarios/{id}
        public async Task<bool> UpdateAsync(UsuarioDTO usuario)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/usuarios/{usuario.Id}", usuario);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // PUT: api/usuarios/{id}/roles
        // 🚀 NUEVO: Envía la lista de IDs de roles al backend para reemplazar los actuales
        public async Task<bool> UpdateRolesAsync(int idUsuario, List<int> rolesIds)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/usuarios/{idUsuario}/roles", rolesIds);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}