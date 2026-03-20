using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class EstadoMateriaService : IEstadoMateriaService
    {
        private readonly HttpClient _http;
        private readonly ILogger<EstadoMateriaService> _logger; // ✅ Agregado para trazabilidad

        public EstadoMateriaService(HttpClient http, ILogger<EstadoMateriaService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<EstadoMateriaDTO>> GetEstadosAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<EstadoMateriaDTO>>("api/estadosmateria");
                return response ?? new List<EstadoMateriaDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de estados de materia.");
                return new List<EstadoMateriaDTO>();
            }
        }

        public async Task<EstadoMateriaDTO?> GetEstadoByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<EstadoMateriaDTO>($"api/estadosmateria/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado de materia con ID {Id}.", id);
                return null;
            }
        }

        public async Task<bool> CrearEstadoAsync(EstadoMateriaDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/estadosmateria", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al crear estado de materia '{Nombre}': {ErrorMsg}", dto.Nombre, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico de conexión al intentar crear un estado de materia.");
                return false;
            }
        }

        public async Task<bool> EditarEstadoAsync(int id, EstadoMateriaDTO dto)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/estadosmateria/{id}", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al actualizar el estado de materia {Id}: {ErrorMsg}", id, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico de conexión al intentar actualizar el estado de materia {Id}.", id);
                return false;
            }
        }

        public async Task<bool> EliminarEstadoAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/estadosmateria/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de red al intentar eliminar el estado de materia {Id}.", id);
                return false;
            }
        }
    }
}