using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace EduSys.Web.Services
{
    public class SedeService : ISedeService
    {
        private readonly HttpClient _http;
        private readonly ILogger<SedeService> _logger; // ✅ Agregado para trazabilidad

        public SedeService(HttpClient http, ILogger<SedeService> logger)
        {
            _http = http;
            _logger = logger;
        }

        // ==========================================
        // GESTIÓN DE SEDES
        // ==========================================

        public async Task<List<SedeDTO>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<SedeDTO>>("api/sedes");
                return response ?? new List<SedeDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista general de sedes.");
                return new List<SedeDTO>();
            }
        }

        public async Task<SedeDTO?> GetByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<SedeDTO>($"api/sedes/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el detalle de la sede con ID {Id}.", id);
                return null;
            }
        }

        public async Task<bool> CreateAsync(SedeDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/sedes", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al crear la sede '{Nombre}': {ErrorMsg}", dto.Nombre, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar crear una sede.");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(SedeDTO dto)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/sedes", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al actualizar la sede {Id}: {ErrorMsg}", dto.Id, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar actualizar la sede {Id}.", dto.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/sedes/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de red al intentar eliminar la sede {Id}.", id);
                return false;
            }
        }

        // ==========================================
        // GESTIÓN DE AULAS
        // ==========================================

        public async Task<List<AulaDTO>> GetAulasBySedeAsync(int idSede)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<AulaDTO>>($"api/sedes/{idSede}/aulas");
                return response ?? new List<AulaDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las aulas de la sede {IdSede}.", idSede);
                return new List<AulaDTO>();
            }
        }

        public async Task<bool> CreateAulaAsync(AulaDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/sedes/aulas", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Fallo al crear un aula en la sede {IdSede}: {ErrorMsg}", dto.IdSede, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo crítico de conexión al intentar crear un aula.");
                return false;
            }
        }

        public async Task<bool> UpdateAulaAsync(AulaDTO dto)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/sedes/aulas", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    // ✅ CORREGIDO: Cambiamos dto.IdAula por dto.Id
                    _logger.LogWarning("Fallo al actualizar el aula {Id}: {ErrorMsg}", dto.Id, errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // ✅ CORREGIDO: Cambiamos dto.IdAula por dto.Id
                _logger.LogError(ex, "Fallo crítico de conexión al intentar actualizar el aula {Id}.", dto.Id);
                return false;
            }
        }

        public async Task<bool> DeleteAulaAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/sedes/aulas/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de red al intentar eliminar el aula {Id}.", id);
                return false;
            }
        }
    }
}