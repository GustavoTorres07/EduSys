using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class CarreraService : ICarreraService
    {
        private readonly HttpClient _http;

        public CarreraService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<CarreraDTO>> GetAllAsync()
        {
            var response = await _http.GetFromJsonAsync<List<CarreraDTO>>("api/carreras");
            return response ?? new List<CarreraDTO>();
        }

        public async Task<CarreraDTO> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<CarreraDTO>($"api/carreras/{id}");
        }

        public async Task<string> CreateAsync(CarreraDTO carrera)
        {
            var response = await _http.PostAsJsonAsync("api/carreras", carrera);

            if (response.IsSuccessStatusCode)
            {
                return string.Empty; // Éxito (cadena vacía)
            }

            // Leemos el error que manda la API
            var errorContent = await response.Content.ReadAsStringAsync();

            // Lógica de limpieza:
            // 1. Si empieza con "{", es un JSON de error de sistema (como el de tu captura)
            if (errorContent.Trim().StartsWith("{"))
            {
                // Devolvemos un mensaje genérico para no asustar al usuario
                return "Error de validación: Verifique que todos los campos obligatorios estén completos y correctos.";
            }

            // 2. Si no es JSON, es nuestro mensaje personalizado (ej: "La carrera ya existe")
            return errorContent;
        }

        public async Task<string> UpdateAsync(CarreraDTO carrera)
        {
            var response = await _http.PutAsJsonAsync("api/carreras", carrera);

            if (response.IsSuccessStatusCode) return string.Empty;

            var errorContent = await response.Content.ReadAsStringAsync();

            if (errorContent.Trim().StartsWith("{"))
            {
                return "Error de validación: Verifique los datos ingresados.";
            }

            return errorContent;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/carreras/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<int>> GetSedesIdsAsync(int carreraId)
        {
            return await _http.GetFromJsonAsync<List<int>>($"api/carreras/{carreraId}/sedes") ?? new List<int>();
        }

        public async Task<bool> ActualizarSedesAsync(int carreraId, List<int> idsSedes)
        {
            var response = await _http.PostAsJsonAsync($"api/carreras/{carreraId}/sedes", idsSedes);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<int>> GetModalidadesIdsAsync(int carreraId)
        {
            return await _http.GetFromJsonAsync<List<int>>($"api/carreras/{carreraId}/modalidades") ?? new List<int>();
        }

        public async Task<bool> ActualizarModalidadesAsync(int carreraId, List<int> idsModalidades)
        {
            var response = await _http.PostAsJsonAsync($"api/carreras/{carreraId}/modalidades", idsModalidades);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<CarreraDTO>> GetCarrerasPorSedeAsync(int idSede)
        {
            return await _http.GetFromJsonAsync<List<CarreraDTO>>($"api/carreras/por-sede/{idSede}")
                   ?? new List<CarreraDTO>();
        }
    }
}