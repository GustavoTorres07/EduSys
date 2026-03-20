using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class ModalidadService : IModalidadService
    {
        private readonly HttpClient _http;
        private readonly ILogger<ModalidadService> _logger; // ✅ Agregado para trazabilidad

        public ModalidadService(HttpClient http, ILogger<ModalidadService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<ModalidadDTO>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<ModalidadDTO>>("api/modalidades");
                return response ?? new List<ModalidadDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el listado de modalidades.");
                return new List<ModalidadDTO>();
            }
        }

        public async Task<ModalidadDTO?> GetByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<ModalidadDTO>($"api/modalidades/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el detalle de la modalidad con ID {Id}.", id);
                return null;
            }
        }

        public async Task<bool> CreateAsync(ModalidadDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/modalidades", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al crear la modalidad '{Nombre}': {ErrorMsg}", dto.Nombre, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar crear una modalidad.");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(ModalidadDTO dto)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/modalidades", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al actualizar la modalidad {Id}: {ErrorMsg}", dto.Id, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar actualizar la modalidad {Id}.", dto.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/modalidades/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de red al intentar eliminar la modalidad {Id}.", id);
                return false;
            }
        }
    }
}