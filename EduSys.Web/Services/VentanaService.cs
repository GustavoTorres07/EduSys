using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class VentanaService : IVentanaService
    {
        private readonly HttpClient _http;
        private readonly ILogger<VentanaService> _logger; // ✅ Inyectado para trazabilidad

        public VentanaService(HttpClient http, ILogger<VentanaService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<VentanaOperativaDTO>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<VentanaOperativaDTO>>("api/ventanas");
                return response ?? new List<VentanaOperativaDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de ventanas operativas.");
                return new List<VentanaOperativaDTO>();
            }
        }

        public async Task<bool> CreateAsync(VentanaOperativaDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/ventanas", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    // ✅ Sin adivinar propiedades, dejamos el log seguro y limpio
                    _logger.LogWarning("Fallo al crear la ventana operativa: {ErrorMsg}", errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar crear una ventana operativa.");
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/ventanas/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Aquí sí usamos la variable 'id' porque nos llega por parámetro al método
                _logger.LogError(ex, "Error de red al intentar eliminar la ventana operativa con ID {Id}.", id);
                return false;
            }
        }
    }
}