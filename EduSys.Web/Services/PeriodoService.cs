using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class PeriodoService : IPeriodoService
    {
        private readonly HttpClient _http;
        private readonly ILogger<PeriodoService> _logger; // ✅ Agregado para trazabilidad

        public PeriodoService(HttpClient http, ILogger<PeriodoService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<PeriodoAcademicoDTO>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<PeriodoAcademicoDTO>>("api/periodos");
                return response ?? new List<PeriodoAcademicoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista general de periodos académicos.");
                return new List<PeriodoAcademicoDTO>();
            }
        }

        public async Task<PeriodoAcademicoDTO?> GetByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<PeriodoAcademicoDTO>($"api/periodos/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el detalle del periodo académico con ID {Id}.", id);
                return null;
            }
        }

        public async Task<bool> CreateAsync(PeriodoAcademicoDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/periodos", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al crear el periodo académico '{Nombre}': {ErrorMsg}", dto.Nombre, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar crear un periodo académico.");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(PeriodoAcademicoDTO dto)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/periodos", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al actualizar el periodo académico {Id}: {ErrorMsg}", dto.Id, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar actualizar el periodo académico {Id}.", dto.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/periodos/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de red al intentar eliminar el periodo académico {Id}.", id);
                return false;
            }
        }
    }
}