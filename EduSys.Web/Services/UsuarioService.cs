using System.Net.Http.Json;
using EduSys.Shared.DTOs;
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

        public async Task<UsuarioDTO?> GetByIdAsync(int id)
        {
            try
            {
                // Hace una petición GET a tu API: ej. https://localhost:7000/api/usuarios/5
                return await _httpClient.GetFromJsonAsync<UsuarioDTO>($"api/usuarios/{id}");
            }
            catch (Exception)
            {
                // Si la API falla o devuelve 404, retornamos null de forma segura
                return null;
            }
        }

        public async Task<bool> UpdateAsync(UsuarioDTO usuario)
        {
            try
            {
                // Hace una petición PUT enviando el DTO modificado
                // Nota: Usamos usuario.Id en la URL o en el cuerpo según cómo esté tu API. 
                // Lo más estándar es mandar el ID por la URL y el objeto en el body:
                var response = await _httpClient.PutAsJsonAsync($"api/usuarios/{usuario.Id}", usuario);

                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                // Si hay error de red, devolvemos falso
                return false;
            }
        }
    }
}