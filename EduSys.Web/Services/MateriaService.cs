using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class MateriaService : IMateriaService
    {
        private readonly HttpClient _http;
        private readonly ILogger<MateriaService> _logger; // ✅ Agregado para trazabilidad

        public MateriaService(HttpClient http, ILogger<MateriaService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<MateriaDTO>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<MateriaDTO>>("api/materias");
                return response ?? new List<MateriaDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el listado general de materias.");
                return new List<MateriaDTO>();
            }
        }

        public async Task<MateriaDTO?> GetByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<MateriaDTO>($"api/materias/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el detalle de la materia con ID {Id}.", id);
                return null;
            }
        }

        public async Task<bool> CreateAsync(MateriaDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/materias", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al crear la materia '{Nombre}': {ErrorMsg}", dto.Nombre, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar crear una materia.");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(MateriaDTO dto)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/materias", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    // ✅ CORREGIDO: Cambiamos dto.IdMateria por dto.Id
                    _logger.LogWarning("Fallo al actualizar la materia {Id}: {ErrorMsg}", dto.Id, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // ✅ CORREGIDO: Cambiamos dto.IdMateria por dto.Id
                _logger.LogError(ex, "Fallo crítico de conexión al intentar actualizar la materia {Id}.", dto.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/materias/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de red al intentar eliminar la materia {Id}.", id);
                return false;
            }
        }
    }
}