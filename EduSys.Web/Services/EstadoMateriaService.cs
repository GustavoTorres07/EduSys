using System.Net.Http.Json;
using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;

namespace EduSys.Web.Services
{
    public class EstadoMateriaService : IEstadoMateriaService
    {
        private readonly HttpClient _http;

        public EstadoMateriaService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<EstadoMateriaDTO>> GetEstadosAsync()
        {
            return await _http.GetFromJsonAsync<List<EstadoMateriaDTO>>("api/estadosmateria") ?? new();
        }

        public async Task<EstadoMateriaDTO?> GetEstadoByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<EstadoMateriaDTO>($"api/estadosmateria/{id}");
        }

        public async Task<bool> CrearEstadoAsync(EstadoMateriaDTO dto)
        {
            var response = await _http.PostAsJsonAsync("api/estadosmateria", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EditarEstadoAsync(int id, EstadoMateriaDTO dto)
        {
            var response = await _http.PutAsJsonAsync($"api/estadosmateria/{id}", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarEstadoAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/estadosmateria/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}