using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class RegimenService : IRegimenService
    {
        private readonly HttpClient _http;
        private readonly ILogger<RegimenService> _logger; // ✅ Inyectado para trazabilidad

        public RegimenService(HttpClient http, ILogger<RegimenService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<RegimenDTO>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<RegimenDTO>>("api/regimenes");
                return response ?? new List<RegimenDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de regímenes.");
                return new List<RegimenDTO>();
            }
        }

        public async Task<bool> CreateAsync(RegimenDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/regimenes", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al crear el régimen: {ErrorMsg}", errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar crear un régimen.");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(RegimenDTO dto)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/regimenes", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al actualizar el régimen: {ErrorMsg}", errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar actualizar el régimen.");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/regimenes/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de red al intentar eliminar el régimen con ID {Id}.", id);
                return false;
            }
        }
    }
}