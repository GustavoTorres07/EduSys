using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json; // ✅ Necesario para leer los errores del backend

namespace EduSys.Web.Services
{
    public class CarreraService : ICarreraService
    {
        private readonly HttpClient _http;
        private readonly ILogger<CarreraService> _logger; // ✅ Inyectamos el logger

        public CarreraService(HttpClient http, ILogger<CarreraService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<CarreraDTO>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<CarreraDTO>>("api/carreras");
                return response ?? new List<CarreraDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de carreras.");
                return new List<CarreraDTO>();
            }
        }

        public async Task<CarreraDTO?> GetByIdAsync(int id)
        {
            try
            {
                return await _http.GetFromJsonAsync<CarreraDTO>($"api/carreras/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la carrera con ID {Id}.", id);
                return null;
            }
        }

        public async Task<string> CreateAsync(CarreraDTO carrera)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/carreras", carrera);
                if (response.IsSuccessStatusCode) return string.Empty; // Éxito

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error creando carrera: {Error}", errorContent);

                return ExtraerMensajeError(errorContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo de conexión al intentar crear la carrera.");
                return "Error de conexión con el servidor.";
            }
        }

        public async Task<string> UpdateAsync(CarreraDTO carrera)
        {
            try
            {
                var response = await _http.PutAsJsonAsync("api/carreras", carrera);
                if (response.IsSuccessStatusCode) return string.Empty;

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error actualizando carrera {Id}: {Error}", carrera.Id, errorContent);

                return ExtraerMensajeError(errorContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo de conexión al intentar actualizar la carrera {Id}.", carrera.Id);
                return "Error de conexión con el servidor.";
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/carreras/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar eliminar la carrera {Id}.", id);
                return false;
            }
        }

        // ==========================================
        // MÉTODOS DE RELACIONES (Sedes y Modalidades)
        // ==========================================

        public async Task<List<int>> GetSedesIdsAsync(int carreraId)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<int>>($"api/carreras/{carreraId}/sedes") ?? new List<int>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sedes de la carrera {Id}.", carreraId);
                return new List<int>();
            }
        }

        public async Task<bool> ActualizarSedesAsync(int carreraId, List<int> idsSedes)
        {
            try
            {
                var response = await _http.PostAsJsonAsync($"api/carreras/{carreraId}/sedes", idsSedes);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar sedes de la carrera {Id}.", carreraId);
                return false;
            }
        }

        public async Task<List<int>> GetModalidadesIdsAsync(int carreraId)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<int>>($"api/carreras/{carreraId}/modalidades") ?? new List<int>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener modalidades de la carrera {Id}.", carreraId);
                return new List<int>();
            }
        }

        public async Task<bool> ActualizarModalidadesAsync(int carreraId, List<int> idsModalidades)
        {
            try
            {
                var response = await _http.PostAsJsonAsync($"api/carreras/{carreraId}/modalidades", idsModalidades);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar modalidades de la carrera {Id}.", carreraId);
                return false;
            }
        }

        public async Task<List<CarreraDTO>> GetCarrerasPorSedeAsync(int idSede)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<CarreraDTO>>($"api/carreras/por-sede/{idSede}") ?? new List<CarreraDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener carreras para la sede {IdSede}.", idSede);
                return new List<CarreraDTO>();
            }
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private string ExtraerMensajeError(string errorContent)
        {
            if (string.IsNullOrWhiteSpace(errorContent)) return "Error desconocido del servidor.";

            if (errorContent.Trim().StartsWith("{"))
            {
                try
                {
                    // Intentamos leer la propiedad "message" del JSON estándar de la API
                    using var jsonDoc = JsonDocument.Parse(errorContent);
                    if (jsonDoc.RootElement.TryGetProperty("message", out var messageProp))
                    {
                        return messageProp.GetString() ?? "Error de validación.";
                    }
                }
                catch
                {
                    // Si falla el parseo, cae al mensaje por defecto 
                }

                // Si es un JSON pero no tiene "message", probablemente sea el BadRequest(ModelState)
                return "Error de validación: Verifique que todos los campos obligatorios estén completos y correctos.";
            }

            // Si es un string simple
            return errorContent;
        }
    }
}