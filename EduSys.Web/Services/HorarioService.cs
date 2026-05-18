using EduSys.Shared.DTOs;
using EduSys.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace EduSys.Web.Services
{
    public class HorarioService : IHorarioService
    {
        private readonly HttpClient _http;
        private readonly ILogger<HorarioService> _logger; // ✅ Inyectamos el logger

        public HorarioService(HttpClient http, ILogger<HorarioService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task<List<HorarioComisionDTO>> GetByComisionAsync(int idComision)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<HorarioComisionDTO>>($"api/horarios/comision/{idComision}")
                       ?? new List<HorarioComisionDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los horarios de la comisión {IdComision}.", idComision);
                return new List<HorarioComisionDTO>();
            }
        }

        public async Task<bool> CreateAsync(HorarioComisionDTO dto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/horarios", dto);

                // Si hay conflicto (Aula ocupada), el backend devuelve 409 Conflict o 400 BadRequest
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var mensajeLimpio = ExtraerMensajeError(errorContent);

                    _logger.LogWarning("Fallo al crear horario: {Mensaje}", mensajeLimpio);

                    // 💡 Lanzamos ApplicationException con el mensaje limpio para tu alerta en la UI
                    throw new ApplicationException(mensajeLimpio);
                }

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Fallo de conexión al intentar crear un horario.");
                throw new ApplicationException("Error de conexión con el servidor.");
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/horarios/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar eliminar el horario {Id}.", id);
                return false;
            }
        }

        public async Task<List<HorarioVisualizacionDTO>> GetVisualizacionAsync(int idPeriodo, int idCarrera, int idSede)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<HorarioVisualizacionDTO>>(
                    $"api/horarios/visualizacion/periodo/{idPeriodo}/carrera/{idCarrera}/sede/{idSede}")
                    ?? new List<HorarioVisualizacionDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la visualización general de horarios.");
                return new List<HorarioVisualizacionDTO>();
            }
        }

        public async Task<List<HorarioVisualizacionDTO>> GetHorariosCursandoAsync(int idPeriodo, int idAlumno)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<HorarioVisualizacionDTO>>(
                    $"api/reportes/horarios-alumno-cursando?idPeriodo={idPeriodo}&idAlumno={idAlumno}")
                    ?? new List<HorarioVisualizacionDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los horarios de cursada del alumno {IdAlumno}.", idAlumno);
                return new List<HorarioVisualizacionDTO>();
            }
        }

        public async Task<byte[]> DescargarPdfAsync(int idPeriodo, int idCarrera, int idSede)
        {
            try
            {
                var url = $"api/reportes/horario-descargar?idPeriodo={idPeriodo}&idCarrera={idCarrera}&idSede={idSede}";
                var response = await _http.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var mensajeLimpio = ExtraerMensajeError(errorContent);

                _logger.LogWarning("Fallo al descargar PDF de horarios: {Mensaje}", mensajeLimpio);
                throw new ApplicationException(mensajeLimpio);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Fallo de conexión al intentar descargar el PDF.");
                throw new ApplicationException("Error de conexión al intentar generar el archivo.");
            }
        }

        public async Task<List<HorarioVisualizacionDTO>> GetMisHorariosAsync()
        {
            try
            {
                // Asumo que crearás esta ruta en el backend, en HorariosController o DocentePortalController.
                // Como el usuario ya está autenticado, la API extrae el ID del token.
                return await _http.GetFromJsonAsync<List<HorarioVisualizacionDTO>>("api/horarios/mis-horarios")
                       ?? new List<HorarioVisualizacionDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los horarios del usuario autenticado.");
                return new List<HorarioVisualizacionDTO>();
            }
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private string ExtraerMensajeError(string errorContent)
        {
            if (string.IsNullOrWhiteSpace(errorContent)) return "Ocurrió un error inesperado en el servidor.";

            if (errorContent.Trim().StartsWith("{"))
            {
                try
                {
                    using var jsonDoc = JsonDocument.Parse(errorContent);
                    if (jsonDoc.RootElement.TryGetProperty("message", out var messageProp))
                    {
                        return messageProp.GetString() ?? "Error de validación en el servidor.";
                    }
                }
                catch
                {
                    // Si el JSON es inválido, ignoramos y devolvemos texto genérico
                }

                return "Error de validación: Verifique los datos ingresados.";
            }

            return errorContent;
        }
    }
}